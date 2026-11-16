using Mediator;
using Noxy.NET.MediaPlayer.Domain.Responses.Remote;

namespace Noxy.NET.MediaPlayer.Application.Notifications.Remote;

public record NotificationPlayerStateChanged(Guid UserID, ResponsePlayerState State) : INotification;
