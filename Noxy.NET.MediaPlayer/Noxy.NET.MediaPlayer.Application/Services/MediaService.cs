using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Noxy.NET.MediaPlayer.Application.Configurations;
using Noxy.NET.MediaPlayer.Application.Helpers;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Interfaces.Gates;
using Noxy.NET.MediaPlayer.Application.Interfaces.Notifiers;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Application.Interfaces.Signals;
using Noxy.NET.MediaPlayer.Application.Interfaces.Validators;
using Noxy.NET.MediaPlayer.Application.Models;
using Noxy.NET.MediaPlayer.Application.Streams;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Extensions;
using Noxy.NET.MediaPlayer.Domain.Requests;
using Noxy.NET.MediaPlayer.Domain.Requests.Media;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;

namespace Noxy.NET.MediaPlayer.Application.Services;

public class MediaService(
    IUnitOfWorkFactory uowFactory,
    IMediaFileValidator validator,
    IFileStorageService storage,
    IUploadSignal signal,
    IOptions<ConfigStorage> storageOptions,
    IOptions<ConfigUpload> uploadOptions,
    IUploadGate gate,
    IMediaNotifier notifier,
    IPlaylistNotifier playlistNotifier,
    IETagService eTagService,
    TimeProvider time,
    ILogger<MediaService> logger) : IMediaService
{
    public async Task<ResponseUploadAccepted> Enqueue(Guid userID, string fileName, string? title, Stream content, CancellationToken cancellationToken = default)
    {
        // One upload per user at a time, held until its job is saved, so the checks below see every earlier upload.
        using IDisposable lease = gate.TryEnter(userID) ?? throw new UploadInProgressException();

        // The body can't be rewound, so the header is read for the check and then replayed in front of the rest.
        byte[] header = new byte[IMediaFileValidator.HeaderSize];
        int headerLength = await content.ReadAtLeastAsync(header, header.Length, false, cancellationToken);
        ValidatedMediaFile file = validator.Validate(fileName, header.AsSpan(0, headerLength));

        long? remainingBytes = await EnsureRoomToEnqueue(userID, cancellationToken);

        StoredFile staged = await Stage(new PrefixedStream(header.AsMemory(0, headerLength), content), file.Extension, remainingBytes, cancellationToken);

        MediaUploadJob job = new()
        {
            UserID = userID,
            StagedFileName = staged.FileName,
            Title = MediaTitle.Resolve(title, fileName),
            ContentType = file.ContentType,
            SizeBytes = staged.SizeBytes,
            ContentHash = staged.ContentHash
        };

        try
        {
            await using IUnitOfWork uow = uowFactory.Create();
            uow.Media.AddMediaUploadJob(job);
            await uow.Commit(cancellationToken);
        }
        catch
        {
            // Don't leave a file in staging that no job points to.
            storage.DeleteStaged(staged.FileName);
            throw;
        }

        signal.Notify();
        return new() { JobID = job.ID };
    }

    public async Task<List<ResponseMediaFile>> GetMediaFileList(Guid userID, int skip, int take, CancellationToken cancellationToken = default)
    {
        await using IUnitOfWork uow = uowFactory.Create();

        List<MediaFile> fileList = await uow.Media.GetMediaFileList(userID, skip, take, cancellationToken);
        return [.. fileList.Select(x => x.ToResponse())];
    }

    public async Task<MediaDownload> Open(Guid userID, Guid id, CancellationToken cancellationToken = default)
    {
        await using IUnitOfWork uow = uowFactory.Create();

        MediaFile mediaFile = await uow.Media.FindMediaFile(id, userID, cancellationToken) ?? throw new MediaNotFoundException();

        // The download is named after the title, with the extension of the stored file.
        string fileName = mediaFile.Metadata is null ? mediaFile.FilePath : $"{mediaFile.Metadata.Title}{Path.GetExtension(mediaFile.FilePath)}";

        try
        {
            return new(storage.OpenRead(mediaFile.FilePath), mediaFile.ContentType, fileName);
        }
        catch (FileNotFoundException)
        {
            throw new MediaNotFoundException();
        }
    }

    public async Task<ResponseMediaFile> Update(Guid userID, Guid id, RequestUpdateMedia request, CancellationToken cancellationToken = default)
    {
        await using IUnitOfWork uow = uowFactory.Create();

        MediaFile file = await uow.Media.FindMediaFile(id, userID, cancellationToken) ?? throw new MediaNotFoundException();
        MediaFileMetadata metadata = file.Metadata ?? throw new MediaNotFoundException();

        bool changed = false;
        if (request.Title.HasValue)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(request.Title.Value, nameof(request));

            changed |= Apply(request.Title.Value.Trim(), metadata.Title, value => metadata.Title = value);
        }

        changed |= ApplyText(request.Artist, metadata.Artist, value => metadata.Artist = value);
        changed |= ApplyText(request.Album, metadata.Album, value => metadata.Album = value);
        changed |= ApplyText(request.AlbumArtist, metadata.AlbumArtist, value => metadata.AlbumArtist = value);
        changed |= ApplyText(request.Subtitle, metadata.Subtitle, value => metadata.Subtitle = value);
        changed |= ApplyText(request.Comment, metadata.Comment, value => metadata.Comment = value);
        changed |= ApplyText(request.Genre, metadata.Genre, value => metadata.Genre = value);
        changed |= ApplyNumber(request.TrackNumber, metadata.TrackNumber, value => metadata.TrackNumber = value);
        changed |= ApplyNumber(request.DiscNumber, metadata.DiscNumber, value => metadata.DiscNumber = value);
        changed |= ApplyNumber(request.Year, metadata.Year, value => metadata.Year = value);

        // An edit that changes nothing leaves the modified time alone, so it can't change the library ETag or push anything.
        if (!changed) return file.ToResponse();

        file.TimeModified = time.GetUtcNow().UtcDateTime;
        uow.Media.UpdateMediaFile(file);
        await uow.Commit(cancellationToken);

        ResponseMediaFile response = file.ToResponse();
        await NotifyQuietly(() => notifier.Updated(userID, response, cancellationToken), id);
        return response;
    }

    public async Task Delete(Guid userID, Guid id, CancellationToken cancellationToken = default)
    {
        await using IUnitOfWork uow = uowFactory.Create();

        MediaFile file = await uow.Media.FindMediaFile(id, userID, cancellationToken) ?? throw new MediaNotFoundException();

        // The song leaves every playlist in the same commit, so no entry is ever left pointing at it.
        DateTime now = time.GetUtcNow().UtcDateTime;
        List<ResponsePlaylist> playlistUpdatedList = [];
        foreach (Playlist playlist in await uow.Playlist.GetPlaylistListByMediaFileID(userID, id, cancellationToken))
        {
            foreach (PlaylistEntry removed in playlist.EntryList.Where(x => x.MediaFileID == id))
            {
                uow.Playlist.RemovePlaylistEntry(removed);
            }

            playlist.EntryList.RemoveAll(x => x.MediaFileID == id);
            foreach (PlaylistEntry changed in playlist.RenumberEntryList())
            {
                uow.Playlist.UpdatePlaylistEntry(changed);
            }

            playlist.TimeModified = now;
            uow.Playlist.UpdatePlaylist(playlist);
            playlistUpdatedList.Add(playlist.ToResponse(playlist.EntryList.Count));
        }

        // The row goes first: a failure after it leaves an orphaned file, never a song that points at nothing.
        uow.Media.RemoveMediaFile(file);
        await uow.Commit(cancellationToken);

        try
        {
            storage.Delete(file.FilePath);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not delete the file of deleted song {MediaFileID}", id);
        }

        await NotifyQuietly(() => notifier.Deleted(userID, id, cancellationToken), id);

        foreach (ResponsePlaylist updated in playlistUpdatedList)
        {
            await NotifyQuietly(() => playlistNotifier.Updated(userID, updated, cancellationToken), updated.ID);
        }
    }

    public async Task<MediaLibrary> GetMediaLibrary(Guid userID, string? ifNoneMatch, CancellationToken cancellationToken = default)
    {
        await using IUnitOfWork uow = uowFactory.Create();

        if (!string.IsNullOrWhiteSpace(ifNoneMatch))
        {
            List<MediaFileStamp> stampList = await uow.Media.GetMediaFileStampList(userID, cancellationToken);
            string current = eTagService.Compute(stampList.Select(x => (x.ID, x.TimeModified)));
            if (eTagService.Matches(ifNoneMatch, current)) return new(current, null);
        }

        // The ETag comes from the list that is returned, so the two can never disagree.
        List<MediaFile> fileList = await uow.Media.GetMediaFileList(userID, cancellationToken);
        return new(eTagService.Compute(fileList.Select(x => (x.ID, x.TimeModified))), [.. fileList.Select(x => x.ToResponse())]);
    }

    private static bool Apply(string value, string? current, Action<string> set)
    {
        if (value == current) return false;

        set(value);
        return true;
    }

    // A blank value clears the tag, so a tag never holds an empty string.
    private static bool ApplyText(Optional<string?> requested, string? current, Action<string?> set)
    {
        if (!requested.HasValue) return false;

        string? value = string.IsNullOrWhiteSpace(requested.Value) ? null : requested.Value.Trim();
        if (value == current) return false;

        set(value);
        return true;
    }

    private static bool ApplyNumber(Optional<int?> requested, int? current, Action<int?> set)
    {
        if (!requested.HasValue || requested.Value == current) return false;

        set(requested.Value);
        return true;
    }

    // A failed notification is only logged: the change itself has already happened.
    private async Task NotifyQuietly(Func<Task> notify, Guid id)
    {
        try
        {
            await notify();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not notify about {EntityID}", id);
        }
    }

    // Stages the file, stopping it as soon as it would pass what the user has left, so it never takes more disk than that.
    private async Task<StoredFile> Stage(Stream content, string extension, long? remainingBytes, CancellationToken cancellationToken)
    {
        if (remainingBytes is not { } remaining) return await storage.StageAsync(content, extension, cancellationToken);

        try
        {
            return await storage.StageAsync(content, extension, remaining, cancellationToken);
        }
        catch (FileTooLargeException) when (remaining < storageOptions.Value.MaxSizeBytes)
        {
            // The quota was the tighter limit, so this is a full library and not an oversized file.
            throw new LibraryFullException();
        }
    }

    // Advisory: it stops a full library or a flood from staging large files in vain. The worker makes the authoritative check.
    // Returns the bytes the user has left, or null when there is no byte limit.
    private async Task<long?> EnsureRoomToEnqueue(Guid userID, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = uowFactory.Create();

        int pending = await uow.Media.CountMediaUploadJobs(userID, cancellationToken);
        int files = await uow.Media.CountMediaFiles(userID, cancellationToken);

        if (files + pending >= storageOptions.Value.MaxFilesPerUser) throw new LibraryFullException();
        if (pending >= uploadOptions.Value.MaxPendingPerUser) throw new UploadQueueFullException();
        if (storageOptions.Value.MaxBytesPerUser == 0) return null;

        long used = await uow.Media.SumMediaFileBytes(userID, cancellationToken) + await uow.Media.SumMediaUploadJobBytes(userID, cancellationToken);
        long remaining = storageOptions.Value.MaxBytesPerUser - used;
        if (remaining <= 0) throw new LibraryFullException();

        return remaining;
    }
}
