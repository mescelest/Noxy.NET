using Mediator;

namespace Noxy.NET.MediaPlayer.Application.Notifications.Remote;

public record NotificationTransferFailed(Guid UserID) : INotification;
