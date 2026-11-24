using Microsoft.AspNetCore.SignalR;
using Noxy.NET.MediaPlayer.API.Handlers.Abstractions;
using Noxy.NET.MediaPlayer.API.Hubs;
using Noxy.NET.MediaPlayer.Application.Notifications.Playlist;
using Noxy.NET.MediaPlayer.Domain.Enums;

namespace Noxy.NET.MediaPlayer.API.Handlers.Playlist;

public class HandlerNotificationPlaylistOrderChanged(IHubContext<MediaHub> hub, ILogger<HandlerNotificationPlaylistOrderChanged> logger) : BaseHandlerNotification<NotificationPlaylistOrderChanged>(hub, logger)
{
    protected override Task Push(IHubContext<MediaHub> hub, NotificationPlaylistOrderChanged notification, CancellationToken cancellationToken)
    {
        return hub.Clients.User(notification.UserID.ToString()).SendAsync(nameof(MediaHubEventEnum.PlaylistOrderChanged), cancellationToken);
    }
}
