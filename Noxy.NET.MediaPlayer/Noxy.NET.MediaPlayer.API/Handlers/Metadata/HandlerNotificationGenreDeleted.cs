using Microsoft.AspNetCore.SignalR;
using Noxy.NET.MediaPlayer.API.Handlers.Abstractions;
using Noxy.NET.MediaPlayer.API.Hubs;
using Noxy.NET.MediaPlayer.Application.Notifications.Metadata;
using Noxy.NET.MediaPlayer.Domain.Enums;

namespace Noxy.NET.MediaPlayer.API.Handlers.Metadata;

public class HandlerNotificationGenreDeleted(IHubContext<MediaHub> hub, ILogger<HandlerNotificationGenreDeleted> logger) : BaseHandlerNotification<NotificationGenreDeleted>(hub, logger)
{
    protected override Task Push(IHubContext<MediaHub> hub, NotificationGenreDeleted notification, CancellationToken cancellationToken)
    {
        return hub.Clients.User(notification.UserID.ToString()).SendAsync(nameof(MediaHubEventEnum.GenreDeleted), notification.ID, cancellationToken);
    }
}
