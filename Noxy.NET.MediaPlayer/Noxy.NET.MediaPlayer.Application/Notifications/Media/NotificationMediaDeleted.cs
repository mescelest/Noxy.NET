using Mediator;

namespace Noxy.NET.MediaPlayer.Application.Notifications.Media;

public record NotificationMediaDeleted(Guid UserID, Guid ID) : INotification;
