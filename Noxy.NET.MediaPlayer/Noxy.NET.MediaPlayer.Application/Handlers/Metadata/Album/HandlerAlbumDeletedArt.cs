using Mediator;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Application.Notifications.Metadata;

namespace Noxy.NET.MediaPlayer.Application.Handlers.Metadata.Album;

// An album goes away by delete or by merge, and both announce it only after the change is saved: the picture file goes then.
public class HandlerAlbumDeletedArt(IArtStorageService serviceArtStorage) : INotificationHandler<NotificationAlbumDeleted>
{
    public ValueTask Handle(NotificationAlbumDeleted notification, CancellationToken cancellationToken)
    {
        serviceArtStorage.DeleteAlbumArt(notification.ID);
        return ValueTask.CompletedTask;
    }
}
