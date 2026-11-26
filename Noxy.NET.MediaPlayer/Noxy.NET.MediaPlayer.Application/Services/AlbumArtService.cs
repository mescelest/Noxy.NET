using Mediator;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Application.Notifications.Media;
using Noxy.NET.MediaPlayer.Application.Notifications.Metadata;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Extensions;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;

namespace Noxy.NET.MediaPlayer.Application.Services;

public class AlbumArtService(
    IUnitOfWorkFactory factoryUnitOfWork,
    IThumbnailService serviceThumbnail,
    IArtStorageService serviceArtStorage,
    IPublisher publisher,
    TimeProvider time) : IAlbumArtService
{
    // Two uploads of one new album can offer a picture at the same moment: one at a time, so the second finds the album already has one.
    private readonly SemaphoreSlim _gate = new(1, 1);

    public async Task<bool> Apply(Guid userID, Guid albumID, byte[] picture, CancellationToken cancellationToken = default)
    {
        return await Store(userID, albumID, picture, false, cancellationToken) == StoreResult.Stored;
    }

    public async Task<bool> Inherit(Guid userID, Guid fromAlbumID, Guid toAlbumID, CancellationToken cancellationToken = default)
    {
        if (fromAlbumID == toAlbumID) return false;

        await _gate.WaitAsync(cancellationToken);
        try
        {
            await using IUnitOfWork uow = factoryUnitOfWork.Create();

            EntityAlbum? from = await uow.Metadata.FindAlbum(fromAlbumID, userID, cancellationToken);
            EntityAlbum? to = await uow.Metadata.FindAlbum(toAlbumID, userID, cancellationToken);
            if (from?.TimeArtModified is null || to is null || to.TimeArtModified is not null) return false;

            try
            {
                await using Stream? source = serviceArtStorage.OpenAlbumArt(fromAlbumID);
                if (source is null) return false;

                using MemoryStream bytes = new();
                await source.CopyToAsync(bytes, cancellationToken);
                await serviceArtStorage.SaveAlbumArt(toAlbumID, bytes.ToArray(), cancellationToken);
            }
            catch (IOException)
            {
                // The edit that asked for this is already done: a picture that cannot be copied is no reason to fail it.
                return false;
            }

            DateTime now = time.GetUtcNow().UtcDateTime;
            to.TimeArtModified = now;
            to.TimeModified = now;
            uow.Metadata.UpdateAlbum(to);
            List<ResponseMediaFile> mediaList = await TouchFiles(uow, userID, toAlbumID, now, cancellationToken);
            await uow.Commit(cancellationToken);

            await publisher.Publish(new NotificationAlbumUpdated(userID, to.ToResponse(mediaList.Count)), cancellationToken);
            await PublishFiles(userID, mediaList, cancellationToken);
            return true;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task Replace(Guid userID, Guid albumID, byte[] picture, CancellationToken cancellationToken = default)
    {
        StoreResult result = await Store(userID, albumID, picture, true, cancellationToken);

        switch (result)
        {
            case StoreResult.Stored:
                return;
            case StoreResult.NoAlbum:
                throw new MetadataItemNotFoundException();
            case StoreResult.NotAnImage:
                throw new InvalidMetadataOperationException("The picture is not an image that can be used.");
            default:
                throw new InvalidOperationException($"The result {result} is not known.");
        }
    }

    private async Task<StoreResult> Store(Guid userID, Guid albumID, byte[] picture, bool replace, CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            await using IUnitOfWork uow = factoryUnitOfWork.Create();

            EntityAlbum? album = await uow.Metadata.FindAlbum(albumID, userID, cancellationToken);
            if (album is null) return StoreResult.NoAlbum;
            if (!replace && album.TimeArtModified is not null) return StoreResult.Kept;

            byte[]? thumbnail = serviceThumbnail.CreateThumbnail(picture);
            if (thumbnail is null) return StoreResult.NotAnImage;

            await serviceArtStorage.SaveAlbumArt(albumID, thumbnail, cancellationToken);

            // The modified time moves with it: a device only takes a pushed album whose modified time is newer than the one it holds.
            DateTime now = time.GetUtcNow().UtcDateTime;
            album.TimeArtModified = now;
            album.TimeModified = now;
            uow.Metadata.UpdateAlbum(album);
            List<ResponseMediaFile> mediaList = await TouchFiles(uow, userID, albumID, now, cancellationToken);
            await uow.Commit(cancellationToken);

            await publisher.Publish(new NotificationAlbumUpdated(userID, album.ToResponse(mediaList.Count)), cancellationToken);
            await PublishFiles(userID, mediaList, cancellationToken);
            return StoreResult.Stored;
        }
        finally
        {
            _gate.Release();
        }
    }

    // A device decides that a file's tags are out of date from the file's modified time, which a new picture does not change by itself: the files of the album get the time of the change, and are pushed after the commit, so every device checks them again.
    private static async Task<List<ResponseMediaFile>> TouchFiles(IUnitOfWork uow, Guid userID, Guid albumID, DateTime now, CancellationToken cancellationToken)
    {
        List<ResponseMediaFile> responseList = [];
        foreach (EntityMediaFile file in await uow.Metadata.GetMediaFileListByAlbum(userID, albumID, cancellationToken))
        {
            file.TimeModified = now;
            uow.Media.UpdateMediaFile(file);
            responseList.Add(file.ToResponse());
        }

        return responseList;
    }

    private async Task PublishFiles(Guid userID, List<ResponseMediaFile> responseList, CancellationToken cancellationToken)
    {
        foreach (ResponseMediaFile response in responseList)
        {
            await publisher.Publish(new NotificationMediaUpdated(userID, response), cancellationToken);
        }
    }

    private enum StoreResult
    {
        Stored,
        NoAlbum,
        Kept,
        NotAnImage
    }
}
