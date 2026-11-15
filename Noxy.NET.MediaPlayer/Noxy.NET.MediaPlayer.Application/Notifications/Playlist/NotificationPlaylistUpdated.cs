using Mediator;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;

namespace Noxy.NET.MediaPlayer.Application.Notifications.Playlist;

public record NotificationPlaylistUpdated(Guid UserID, ResponsePlaylist Playlist) : INotification;
