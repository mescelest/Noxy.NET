using Microsoft.AspNetCore.SignalR;
using Noxy.NET.MediaPlayer.API.Handlers.Abstractions;
using Noxy.NET.MediaPlayer.API.Hubs;
using Noxy.NET.MediaPlayer.Application.Notifications.Playback;
using Noxy.NET.MediaPlayer.Domain.Enums;

namespace Noxy.NET.MediaPlayer.API.Handlers.Playback;

public class HandlerNotificationPlaybackSessionUpdated(IHubContext<MediaHub> hub, ILogger<HandlerNotificationPlaybackSessionUpdated> logger) : BaseHandlerNotification<NotificationPlaybackSessionUpdated>(hub, logger)
{
    protected override Task Push(IHubContext<MediaHub> hub, NotificationPlaybackSessionUpdated notification, CancellationToken cancellationToken)
    {
        return hub.Clients.User(notification.UserID.ToString()).SendAsync(nameof(MediaHubEventEnum.PlaybackSessionUpdated), notification.Session, cancellationToken);
    }
}
