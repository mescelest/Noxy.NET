using Mediator;
using Noxy.NET.MediaPlayer.Application.Models.Metadata;
using Noxy.NET.MediaPlayer.Application.Notifications.Metadata;
using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;

namespace Noxy.NET.MediaPlayer.Application.Extensions;

public static class PublisherExtension
{
    extension(IPublisher publisher)
    {
        // The items of the change set, updated ones first and then deleted ones. Call it after the commit, so a device is never told about a change that was not saved.
        public async Task PublishItemList(Guid userID, MetadataChangeSet changeSet, CancellationToken cancellationToken)
        {
            foreach (ResponseArtist artist in changeSet.ArtistUpdatedList) await publisher.Publish(new NotificationArtistUpdated(userID, artist), cancellationToken);
            foreach (ResponseGenre genre in changeSet.GenreUpdatedList) await publisher.Publish(new NotificationGenreUpdated(userID, genre), cancellationToken);
            foreach (ResponseAlbum album in changeSet.AlbumUpdatedList) await publisher.Publish(new NotificationAlbumUpdated(userID, album), cancellationToken);
            foreach (Guid id in changeSet.ArtistDeletedIDList) await publisher.Publish(new NotificationArtistDeleted(userID, id), cancellationToken);
            foreach (Guid id in changeSet.GenreDeletedIDList) await publisher.Publish(new NotificationGenreDeleted(userID, id), cancellationToken);
            foreach (Guid id in changeSet.AlbumDeletedIDList) await publisher.Publish(new NotificationAlbumDeleted(userID, id), cancellationToken);
        }
    }
}
