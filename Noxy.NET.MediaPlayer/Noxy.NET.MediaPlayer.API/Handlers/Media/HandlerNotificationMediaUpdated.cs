using Microsoft.AspNetCore.SignalR;
using Noxy.NET.MediaPlayer.API.Handlers.Abstractions;
using Noxy.NET.MediaPlayer.API.Hubs;
using Noxy.NET.MediaPlayer.Application.Notifications.Media;
using Noxy.NET.MediaPlayer.Domain.Enums;

namespace Noxy.NET.MediaPlayer.API.Handlers.Media;

public class HandlerNotificationMediaUpdated(IHubContext<MediaHub> hub, ILogger<HandlerNotificationMediaUpdated> logger) : BaseHandlerNotification<NotificationMediaUpdated>(hub, logger)
{
    protected override Task Push(IHubContext<MediaHub> hub, NotificationMediaUpdated notification, CancellationToken cancellationToken)
    {
        return hub.Clients.User(notification.UserID.ToString()).SendAsync(nameof(MediaHubEventEnum.MediaUpdated), notification.MediaFile, cancellationToken);
    }
}
