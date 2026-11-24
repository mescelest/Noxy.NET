using Mediator;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Application.Notifications.Metadata;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Extensions;

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
        await _gate.WaitAsync(cancellationToken);
        try
        {
            await using IUnitOfWork uow = factoryUnitOfWork.Create();

            EntityAlbum? album = await uow.Metadata.FindAlbum(albumID, userID, cancellationToken);
            if (album is null || album.TimeArtModified is not null) return false;

            byte[]? thumbnail = serviceThumbnail.CreateThumbnail(picture);
            if (thumbnail is null) return false;

            await serviceArtStorage.SaveAlbumArt(albumID, thumbnail, cancellationToken);

            // The modified time moves with it: a device only takes a pushed album whose modified time is newer than the one it holds.
            DateTime now = time.GetUtcNow().UtcDateTime;
            album.TimeArtModified = now;
            album.TimeModified = now;
            uow.Metadata.UpdateAlbum(album);
            int mediaFileCount = (await uow.Metadata.GetMediaFileListByAlbum(userID, albumID, cancellationToken)).Count;
            await uow.Commit(cancellationToken);

            await publisher.Publish(new NotificationAlbumUpdated(userID, album.ToResponse(mediaFileCount)), cancellationToken);
            return true;
        }
        finally
        {
            _gate.Release();
        }
    }
}
