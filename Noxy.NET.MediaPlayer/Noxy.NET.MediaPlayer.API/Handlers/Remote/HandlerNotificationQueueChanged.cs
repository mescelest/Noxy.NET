using Microsoft.AspNetCore.SignalR;
using Noxy.NET.MediaPlayer.API.Handlers.Abstractions;
using Noxy.NET.MediaPlayer.API.Hubs;
using Noxy.NET.MediaPlayer.Application.Notifications.Remote;
using Noxy.NET.MediaPlayer.Domain.Enums;

namespace Noxy.NET.MediaPlayer.API.Handlers.Remote;

public class HandlerNotificationQueueChanged(IHubContext<MediaHub> hub, ILogger<HandlerNotificationQueueChanged> logger) : BaseHandlerNotification<NotificationQueueChanged>(hub, logger)
{
    protected override Task Push(IHubContext<MediaHub> hub, NotificationQueueChanged notification, CancellationToken cancellationToken)
    {
        return hub.Clients.User(notification.UserID.ToString()).SendAsync(nameof(MediaHubEventEnum.QueueChanged), notification.Delta, cancellationToken);
    }
}
