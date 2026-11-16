using Microsoft.AspNetCore.SignalR;
using Noxy.NET.MediaPlayer.API.Handlers.Abstractions;
using Noxy.NET.MediaPlayer.API.Hubs;
using Noxy.NET.MediaPlayer.Application.Notifications.Remote;
using Noxy.NET.MediaPlayer.Domain.Enums;

namespace Noxy.NET.MediaPlayer.API.Handlers.Remote;

public class HandlerNotificationDeviceListChanged(IHubContext<MediaHub> hub, ILogger<HandlerNotificationDeviceListChanged> logger) : BaseHandlerNotification<NotificationDeviceListChanged>(hub, logger)
{
    protected override Task Push(IHubContext<MediaHub> hub, NotificationDeviceListChanged notification, CancellationToken cancellationToken)
    {
        return hub.Clients.User(notification.UserID.ToString()).SendAsync(nameof(MediaHubEventEnum.DeviceListChanged), notification.DeviceList, cancellationToken);
    }
}
