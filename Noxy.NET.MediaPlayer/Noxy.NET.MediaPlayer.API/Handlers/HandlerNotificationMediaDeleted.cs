using Microsoft.AspNetCore.SignalR;
using Noxy.NET.MediaPlayer.API.Hubs;
using Noxy.NET.MediaPlayer.Application.Notifications.Media;
using Noxy.NET.MediaPlayer.Domain.Enums;

namespace Noxy.NET.MediaPlayer.API.Handlers;

public class HandlerNotificationMediaDeleted(IHubContext<MediaHub> hub, ILogger<HandlerNotificationMediaDeleted> logger) : HandlerNotification<NotificationMediaDeleted>(hub, logger)
{
    protected override Task Push(IHubContext<MediaHub> hub, NotificationMediaDeleted notification, CancellationToken cancellationToken)
    {
        return hub.Clients.User(notification.UserID.ToString()).SendAsync(nameof(MediaHubEvent.MediaDeleted), notification.ID, cancellationToken);
    }
}
