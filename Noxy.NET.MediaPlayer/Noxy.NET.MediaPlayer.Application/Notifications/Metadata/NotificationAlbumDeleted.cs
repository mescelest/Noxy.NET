using Mediator;

namespace Noxy.NET.MediaPlayer.Application.Notifications.Metadata;

public record NotificationAlbumDeleted(Guid UserID, Guid ID) : INotification;
