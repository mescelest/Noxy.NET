using Mediator;

namespace Noxy.NET.MediaPlayer.Application.Notifications.Playback;

public record NotificationPlaybackSessionCleared(Guid UserID) : INotification;
