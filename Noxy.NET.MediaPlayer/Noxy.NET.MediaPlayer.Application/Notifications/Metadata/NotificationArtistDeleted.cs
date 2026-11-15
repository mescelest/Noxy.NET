using Mediator;

namespace Noxy.NET.MediaPlayer.Application.Notifications.Metadata;

public record NotificationArtistDeleted(Guid UserID, Guid ID) : INotification;
