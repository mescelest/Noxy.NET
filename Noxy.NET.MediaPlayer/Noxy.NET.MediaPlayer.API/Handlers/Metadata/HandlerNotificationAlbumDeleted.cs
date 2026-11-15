using Microsoft.AspNetCore.SignalR;
using Noxy.NET.MediaPlayer.API.Handlers.Abstractions;
using Noxy.NET.MediaPlayer.API.Hubs;
using Noxy.NET.MediaPlayer.Application.Notifications.Metadata;
using Noxy.NET.MediaPlayer.Domain.Enums;

namespace Noxy.NET.MediaPlayer.API.Handlers.Metadata;

public class HandlerNotificationAlbumDeleted(IHubContext<MediaHub> hub, ILogger<HandlerNotificationAlbumDeleted> logger) : BaseHandlerNotification<NotificationAlbumDeleted>(hub, logger)
{
    protected override Task Push(IHubContext<MediaHub> hub, NotificationAlbumDeleted notification, CancellationToken cancellationToken)
    {
        return hub.Clients.User(notification.UserID.ToString()).SendAsync(nameof(MediaHubEvent.AlbumDeleted), notification.ID, cancellationToken);
    }
}
