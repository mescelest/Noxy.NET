using Microsoft.AspNetCore.SignalR;
using Noxy.NET.MediaPlayer.API.Handlers.Abstractions;
using Noxy.NET.MediaPlayer.API.Hubs;
using Noxy.NET.MediaPlayer.Application.Notifications.Metadata;
using Noxy.NET.MediaPlayer.Domain.Enums;

namespace Noxy.NET.MediaPlayer.API.Handlers.Metadata;

public class HandlerNotificationGenreUpdated(IHubContext<MediaHub> hub, ILogger<HandlerNotificationGenreUpdated> logger) : BaseHandlerNotification<NotificationGenreUpdated>(hub, logger)
{
    protected override Task Push(IHubContext<MediaHub> hub, NotificationGenreUpdated notification, CancellationToken cancellationToken)
    {
        return hub.Clients.User(notification.UserID.ToString()).SendAsync(nameof(MediaHubEventEnum.GenreUpdated), notification.Genre, cancellationToken);
    }
}
