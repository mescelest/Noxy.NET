using Microsoft.AspNetCore.SignalR;
using Noxy.NET.MediaPlayer.API.Hubs;
using Noxy.NET.MediaPlayer.Application.Notifications.Playlist;
using Noxy.NET.MediaPlayer.Domain.Enums;

namespace Noxy.NET.MediaPlayer.API.Handlers;

public class HandlerNotificationPlaylistUpdated(IHubContext<MediaHub> hub, ILogger<HandlerNotificationPlaylistUpdated> logger) : HandlerNotification<NotificationPlaylistUpdated>(hub, logger)
{
    protected override Task Push(IHubContext<MediaHub> hub, NotificationPlaylistUpdated notification, CancellationToken cancellationToken)
    {
        return hub.Clients.User(notification.UserID.ToString()).SendAsync(nameof(MediaHubEvent.PlaylistUpdated), notification.Playlist, cancellationToken);
    }
}
