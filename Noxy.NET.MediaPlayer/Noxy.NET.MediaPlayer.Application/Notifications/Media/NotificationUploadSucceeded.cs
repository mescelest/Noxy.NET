using Mediator;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;

namespace Noxy.NET.MediaPlayer.Application.Notifications.Media;

public record NotificationUploadSucceeded(Guid UserID, ResponseUploadResult Result) : INotification;
