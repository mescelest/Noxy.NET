using Microsoft.AspNetCore.SignalR;
using Noxy.NET.MediaPlayer.API.Hubs;
using Noxy.NET.MediaPlayer.Application.Notifications.Media;
using Noxy.NET.MediaPlayer.Domain.Enums;

namespace Noxy.NET.MediaPlayer.API.Handlers;

public class HandlerNotificationUploadSucceeded(IHubContext<MediaHub> hub, ILogger<HandlerNotificationUploadSucceeded> logger) : HandlerNotification<NotificationUploadSucceeded>(hub, logger)
{
    protected override Task Push(IHubContext<MediaHub> hub, NotificationUploadSucceeded notification, CancellationToken cancellationToken)
    {
        return hub.Clients.User(notification.UserID.ToString()).SendAsync(nameof(MediaHubEvent.UploadSucceeded), notification.Result, cancellationToken);
    }
}
