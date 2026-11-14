using Noxy.NET.MediaPlayer.Application.Interfaces.Notifiers;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakePlaylistNotifier : IPlaylistNotifier
{
    public List<(Guid UserID, ResponsePlaylist Playlist)> UpdatedList { get; } = [];
    public List<(Guid UserID, Guid ID)> DeletedList { get; } = [];
    public Exception? Failure { get; set; }

    public Task Updated(Guid userID, ResponsePlaylist playlist, CancellationToken cancellationToken = default)
    {
        if (Failure is not null) throw Failure;

        UpdatedList.Add((userID, playlist));
        return Task.CompletedTask;
    }

    public Task Deleted(Guid userID, Guid id, CancellationToken cancellationToken = default)
    {
        if (Failure is not null) throw Failure;

        DeletedList.Add((userID, id));
        return Task.CompletedTask;
    }
}
