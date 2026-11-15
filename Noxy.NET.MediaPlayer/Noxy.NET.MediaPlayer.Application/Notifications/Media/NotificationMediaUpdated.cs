using Mediator;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;

namespace Noxy.NET.MediaPlayer.Application.Notifications.Media;

public record NotificationMediaUpdated(Guid UserID, ResponseMediaFile MediaFile) : INotification;
