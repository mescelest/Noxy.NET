using Mediator;

namespace Noxy.NET.MediaPlayer.Application.Notifications.Playlist;

public record NotificationPlaylistDeleted(Guid UserID, Guid ID) : INotification;
