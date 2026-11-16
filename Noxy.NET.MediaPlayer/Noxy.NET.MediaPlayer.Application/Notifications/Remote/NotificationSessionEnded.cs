using Mediator;

namespace Noxy.NET.MediaPlayer.Application.Notifications.Remote;

public record NotificationSessionEnded(Guid UserID) : INotification;
