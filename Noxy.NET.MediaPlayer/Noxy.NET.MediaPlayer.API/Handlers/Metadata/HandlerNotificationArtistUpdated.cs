using Microsoft.AspNetCore.SignalR;
using Noxy.NET.MediaPlayer.API.Handlers.Abstractions;
using Noxy.NET.MediaPlayer.API.Hubs;
using Noxy.NET.MediaPlayer.Application.Notifications.Metadata;
using Noxy.NET.MediaPlayer.Domain.Enums;

namespace Noxy.NET.MediaPlayer.API.Handlers.Metadata;

public class HandlerNotificationArtistUpdated(IHubContext<MediaHub> hub, ILogger<HandlerNotificationArtistUpdated> logger) : BaseHandlerNotification<NotificationArtistUpdated>(hub, logger)
{
    protected override Task Push(IHubContext<MediaHub> hub, NotificationArtistUpdated notification, CancellationToken cancellationToken)
    {
        return hub.Clients.User(notification.UserID.ToString()).SendAsync(nameof(MediaHubEvent.ArtistUpdated), notification.Artist, cancellationToken);
    }
}
