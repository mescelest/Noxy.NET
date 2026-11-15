using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Noxy.NET.MediaPlayer.Application.Extensions;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Application.Models.Metadata;
using Noxy.NET.MediaPlayer.Domain.Entities;

namespace Noxy.NET.MediaPlayer.Application.BackgroundServices;

public class MetadataBackfillService(IUnitOfWorkFactory factoryUnitOfWork, IMetadataResolutionService serviceMetadataResolution, TimeProvider time, ILogger<MetadataBackfillService> logger) : BackgroundService
{
    public const int BatchSize = 200;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Don't hold up the start of the host with the work.
        await Task.Yield();

        try
        {
            await Run(stoppingToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            // The next start-up continues it.
            logger.LogError(exception, "The metadata backfill failed.");
        }
    }

    // Goes through the files that held legacy text when it started, in ID order, a batch of files in one commit. A batch that fails is logged and left for the next start-up, and the work goes on with the next one. Returns how many files it changed.
    public async Task<int> Run(CancellationToken cancellationToken = default)
    {
        int changedCount = 0;
        List<Guid> idList = await ReadIDList(cancellationToken);

        foreach (Guid[] batch in idList.Chunk(BatchSize))
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                // Another request may create the same artist first: then the batch runs again and reuses it.
                changedCount += await factoryUnitOfWork.RunWithKeyConflictRetry(uow => ProcessBatch(uow, [.. batch], cancellationToken), cancellationToken);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                logger.LogError(exception, "A batch of {Size} files of the metadata backfill failed and is left for the next start-up.", batch.Length);
            }
        }

        logger.LogInformation("The metadata backfill changed {Count} files.", changedCount);
        return changedCount;
    }

    private async Task<List<Guid>> ReadIDList(CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = factoryUnitOfWork.Create();

        return await uow.Media.GetMediaFileIDListWithLegacyText(cancellationToken);
    }

    private async Task<int> ProcessBatch(IUnitOfWork uow, List<Guid> idList, CancellationToken cancellationToken)
    {
        DateTime now = time.GetUtcNow().UtcDateTime;
        int changedCount = 0;

        foreach (EntityMediaFile file in await uow.Media.GetMediaFileListByID(idList, cancellationToken))
        {
            if (await Backfill(uow, file, now, cancellationToken)) changedCount++;
        }

        if (changedCount > 0) await uow.Commit(cancellationToken);

        return changedCount;
    }

    // Links what the legacy text of each kind says, unless the file has items of that kind already, then clears the legacy text. Album artist text with no album to belong to is kept, so no text is lost. Returns whether anything changed.
    private async Task<bool> Backfill(IUnitOfWork uow, EntityMediaFile file, DateTime now, CancellationToken cancellationToken)
    {
        EntityMediaFileMetadata? metadata = file.Metadata;
        if (metadata is null) return false;

        bool linkArtist = !string.IsNullOrWhiteSpace(metadata.Artist) && file.ArtistLinkList.Count == 0;
        bool linkGenre = !string.IsNullOrWhiteSpace(metadata.Genre) && file.GenreLinkList.Count == 0;
        bool linkAlbum = !string.IsNullOrWhiteSpace(metadata.Album) && file.AlbumID is null;

        MediaTagSet tags = new(
            null,
            linkArtist && metadata.Artist is not null ? [metadata.Artist] : [],
            linkAlbum ? metadata.Album : null,
            linkAlbum && metadata.AlbumArtist is not null ? [metadata.AlbumArtist] : [],
            linkGenre && metadata.Genre is not null ? [metadata.Genre] : [],
            null, null, null, null, null, null);
        ResolvedMetadata resolved = await serviceMetadataResolution.Resolve(uow, file.UserID, tags, cancellationToken);

        bool changed = false;
        if (linkArtist) changed |= uow.SetArtistList(file, resolved.ArtistList);
        if (linkGenre) changed |= uow.SetGenreList(file, resolved.GenreList);
        if (linkAlbum && resolved.Album is not null)
        {
            file.AlbumID = resolved.Album.ID;
            file.Album = resolved.Album;
            changed = true;
        }

        if (metadata.Artist is not null)
        {
            metadata.Artist = null;
            changed = true;
        }

        if (metadata.Genre is not null)
        {
            metadata.Genre = null;
            changed = true;
        }

        if (metadata.Album is not null)
        {
            metadata.Album = null;
            changed = true;
        }

        // The album artist belongs to the album: without one it would show nowhere, so its text stays where it is.
        if (metadata.AlbumArtist is not null && (file.AlbumID is not null || string.IsNullOrWhiteSpace(metadata.AlbumArtist)))
        {
            metadata.AlbumArtist = null;
            changed = true;
        }

        if (!changed) return false;

        file.TimeModified = now;
        uow.Media.UpdateMediaFile(file);
        return true;
    }
}
