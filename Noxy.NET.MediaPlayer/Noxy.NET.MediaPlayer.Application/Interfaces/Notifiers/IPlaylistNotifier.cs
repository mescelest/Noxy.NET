using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;

namespace Noxy.NET.MediaPlayer.Application.Interfaces.Notifiers;

public interface IPlaylistNotifier
{
    // Tells the user's connected devices a playlist changed, with its new state but without its entries. A user with no live connection is not an error.
    Task Updated(Guid userID, ResponsePlaylist playlist, CancellationToken cancellationToken = default);

    Task Deleted(Guid userID, Guid id, CancellationToken cancellationToken = default);
}
