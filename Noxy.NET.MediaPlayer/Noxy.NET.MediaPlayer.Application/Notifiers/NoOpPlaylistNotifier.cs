using Noxy.NET.MediaPlayer.Application.Interfaces.Notifiers;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;

namespace Noxy.NET.MediaPlayer.Application.Notifiers;

public class NoOpPlaylistNotifier : IPlaylistNotifier
{
    public Task Updated(Guid userID, ResponsePlaylist playlist, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task Deleted(Guid userID, Guid id, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
