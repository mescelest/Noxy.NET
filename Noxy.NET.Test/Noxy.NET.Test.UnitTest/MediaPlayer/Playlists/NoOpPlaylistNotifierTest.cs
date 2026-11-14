using Noxy.NET.MediaPlayer.Application.Notifiers;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;

namespace Noxy.NET.Test.MediaPlayer.Playlists;

public class NoOpPlaylistNotifierTest
{
    [Fact]
    public async Task Updated_And_Deleted_CompleteWithoutThrowing()
    {
        NoOpPlaylistNotifier notifier = new();
        ResponsePlaylist playlist = new()
        {
            ID = Guid.CreateVersion7(),
            Name = "Mix",
            AllowDuplicates = true,
            EntryCount = 0,
            TimeCreated = DateTime.UtcNow,
            TimeModified = DateTime.UtcNow
        };

        await notifier.Updated(Guid.CreateVersion7(), playlist, TestContext.Current.CancellationToken);
        await notifier.Deleted(Guid.CreateVersion7(), playlist.ID, TestContext.Current.CancellationToken);
    }
}
