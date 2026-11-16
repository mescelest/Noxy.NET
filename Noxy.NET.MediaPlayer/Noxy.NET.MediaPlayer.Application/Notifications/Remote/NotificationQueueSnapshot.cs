using Mediator;
using Noxy.NET.MediaPlayer.Domain.Responses.Remote;

namespace Noxy.NET.MediaPlayer.Application.Notifications.Remote;

public record NotificationQueueSnapshot(Guid UserID, ResponseQueue Queue) : INotification;
