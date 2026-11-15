using Mediator;
using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;

namespace Noxy.NET.MediaPlayer.Application.Notifications.Metadata;

public record NotificationGenreUpdated(Guid UserID, ResponseGenre Genre) : INotification;
