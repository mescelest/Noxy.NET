using Microsoft.AspNetCore.SignalR;
using Noxy.NET.MediaPlayer.API.Handlers.Abstractions;
using Noxy.NET.MediaPlayer.API.Hubs;
using Noxy.NET.MediaPlayer.Application.Notifications.Playback;
using Noxy.NET.MediaPlayer.Domain.Enums;

namespace Noxy.NET.MediaPlayer.API.Handlers.Playback;

public class HandlerNotificationPlaybackSessionCleared(IHubContext<MediaHub> hub, ILogger<HandlerNotificationPlaybackSessionCleared> logger) : BaseHandlerNotification<NotificationPlaybackSessionCleared>(hub, logger)
{
    protected override Task Push(IHubContext<MediaHub> hub, NotificationPlaybackSessionCleared notification, CancellationToken cancellationToken)
    {
        return hub.Clients.User(notification.UserID.ToString()).SendAsync(nameof(MediaHubEventEnum.PlaybackSessionCleared), cancellationToken);
    }
}
