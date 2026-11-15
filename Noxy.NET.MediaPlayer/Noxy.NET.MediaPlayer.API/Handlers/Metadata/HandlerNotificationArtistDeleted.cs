using Microsoft.AspNetCore.SignalR;
using Noxy.NET.MediaPlayer.API.Handlers.Abstractions;
using Noxy.NET.MediaPlayer.API.Hubs;
using Noxy.NET.MediaPlayer.Application.Notifications.Metadata;
using Noxy.NET.MediaPlayer.Domain.Enums;

namespace Noxy.NET.MediaPlayer.API.Handlers.Metadata;

public class HandlerNotificationArtistDeleted(IHubContext<MediaHub> hub, ILogger<HandlerNotificationArtistDeleted> logger) : BaseHandlerNotification<NotificationArtistDeleted>(hub, logger)
{
    protected override Task Push(IHubContext<MediaHub> hub, NotificationArtistDeleted notification, CancellationToken cancellationToken)
    {
        return hub.Clients.User(notification.UserID.ToString()).SendAsync(nameof(MediaHubEvent.ArtistDeleted), notification.ID, cancellationToken);
    }
}
