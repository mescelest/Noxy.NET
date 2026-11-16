using Microsoft.AspNetCore.SignalR;
using Noxy.NET.MediaPlayer.API.Handlers.Abstractions;
using Noxy.NET.MediaPlayer.API.Hubs;
using Noxy.NET.MediaPlayer.Application.Notifications.Setting;
using Noxy.NET.MediaPlayer.Domain.Enums;

namespace Noxy.NET.MediaPlayer.API.Handlers.Setting;

public class HandlerNotificationSettingUpdated(IHubContext<MediaHub> hub, ILogger<HandlerNotificationSettingUpdated> logger) : BaseHandlerNotification<NotificationSettingUpdated>(hub, logger)
{
    protected override Task Push(IHubContext<MediaHub> hub, NotificationSettingUpdated notification, CancellationToken cancellationToken)
    {
        return hub.Clients.User(notification.UserID.ToString()).SendAsync(nameof(MediaHubEventEnum.SettingUpdated), notification.Setting, cancellationToken);
    }
}
