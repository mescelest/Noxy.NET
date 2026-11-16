using Microsoft.AspNetCore.SignalR;
using Noxy.NET.MediaPlayer.API.Handlers.Abstractions;
using Noxy.NET.MediaPlayer.API.Hubs;
using Noxy.NET.MediaPlayer.Application.Notifications.Remote;
using Noxy.NET.MediaPlayer.Domain.Enums;

namespace Noxy.NET.MediaPlayer.API.Handlers.Remote;

public class HandlerNotificationTransferFailed(IHubContext<MediaHub> hub, ILogger<HandlerNotificationTransferFailed> logger) : BaseHandlerNotification<NotificationTransferFailed>(hub, logger)
{
    protected override Task Push(IHubContext<MediaHub> hub, NotificationTransferFailed notification, CancellationToken cancellationToken)
    {
        return hub.Clients.User(notification.UserID.ToString()).SendAsync(nameof(MediaHubEventEnum.TransferFailed), cancellationToken);
    }
}
