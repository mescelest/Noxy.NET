using Mediator;

namespace Noxy.NET.MediaPlayer.Application.Notifications.Metadata;

public record NotificationGenreDeleted(Guid UserID, Guid ID) : INotification;
