using Microsoft.AspNetCore.SignalR;
using Noxy.NET.MediaPlayer.API.Hubs;
using Noxy.NET.MediaPlayer.Application.Notifications.Media;
using Noxy.NET.MediaPlayer.Domain.Enums;

namespace Noxy.NET.MediaPlayer.API.Handlers;

public class HandlerNotificationUploadFailed(IHubContext<MediaHub> hub, ILogger<HandlerNotificationUploadFailed> logger) : HandlerNotification<NotificationUploadFailed>(hub, logger)
{
    protected override Task Push(IHubContext<MediaHub> hub, NotificationUploadFailed notification, CancellationToken cancellationToken)
    {
        return hub.Clients.User(notification.UserID.ToString()).SendAsync(nameof(MediaHubEvent.UploadFailed), notification.Result, cancellationToken);
    }
}
