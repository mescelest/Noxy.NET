using Microsoft.AspNetCore.SignalR;
using Noxy.NET.MediaPlayer.API.Handlers.Abstractions;
using Noxy.NET.MediaPlayer.API.Hubs;
using Noxy.NET.MediaPlayer.Application.Notifications.Media;
using Noxy.NET.MediaPlayer.Domain.Enums;

namespace Noxy.NET.MediaPlayer.API.Handlers.Media;

public class HandlerNotificationMediaDeleted(IHubContext<MediaHub> hub, ILogger<HandlerNotificationMediaDeleted> logger) : BaseHandlerNotification<NotificationMediaDeleted>(hub, logger)
{
    protected override Task Push(IHubContext<MediaHub> hub, NotificationMediaDeleted notification, CancellationToken cancellationToken)
    {
        return hub.Clients.User(notification.UserID.ToString()).SendAsync(nameof(MediaHubEventEnum.MediaDeleted), notification.ID, cancellationToken);
    }
}
