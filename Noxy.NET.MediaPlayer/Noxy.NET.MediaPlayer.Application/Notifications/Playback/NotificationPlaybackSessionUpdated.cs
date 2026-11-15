using Mediator;
using Noxy.NET.MediaPlayer.Domain.Responses.Playback;

namespace Noxy.NET.MediaPlayer.Application.Notifications.Playback;

public record NotificationPlaybackSessionUpdated(Guid UserID, ResponsePlaybackSession Session) : INotification;
