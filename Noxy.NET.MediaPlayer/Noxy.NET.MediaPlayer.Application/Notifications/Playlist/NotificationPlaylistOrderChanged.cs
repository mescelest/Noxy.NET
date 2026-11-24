using Mediator;

namespace Noxy.NET.MediaPlayer.Application.Notifications.Playlist;

// The positions of the user's playlists changed (a move, or a new playlist on top), so a device reads the list again.
public record NotificationPlaylistOrderChanged(Guid UserID) : INotification;
