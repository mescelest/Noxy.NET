using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Models;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;
using Noxy.NET.MediaPlayer.Presentation.Enums;
using Noxy.NET.MediaPlayer.Presentation.Features.Download;
using Noxy.NET.MediaPlayer.Presentation.Features.Player;
using Noxy.NET.MediaPlayer.Presentation.Features.PlaylistList;
using Noxy.NET.MediaPlayer.Presentation.Features.Session;
using Noxy.NET.MediaPlayer.Presentation.Features.Shell;
using Noxy.NET.MediaPlayer.Presentation.Models;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class FeatureSmallReducersTest
{
    [Fact]
    public void PlaylistListLoaded_ReplacesTheList()
    {
        ResponsePlaylist playlist = new() { ID = Guid.NewGuid(), Name = "Mix", AllowDuplicates = true, EntryCount = 0, TimeCreated = DateTime.UtcNow, TimeModified = DateTime.UtcNow, TimeLastPlayed = null };

        FeaturePlaylistListState state = FeaturePlaylistListReducers.ReduceLoaded(new(), new([playlist]));
        Assert.Equal([playlist], state.PlaylistList);

        Assert.Empty(FeaturePlaylistListReducers.ReduceLoaded(state, new([])).PlaylistList);
    }

    [Fact]
    public void SessionChanged_ReplacesEveryFlag()
    {
        FeatureSessionState state = FeatureSessionReducers.ReduceChanged(new(), new(true, true, true, true, true));

        Assert.True(state.IsSignedIn);
        Assert.True(state.HasSession);
        Assert.True(state.IsOffline);
        Assert.True(state.IsRetrying);
        Assert.True(state.IsRestored);

        state = FeatureSessionReducers.ReduceChanged(state, new(false, false, false, false, false));

        Assert.False(state.IsSignedIn || state.HasSession || state.IsOffline || state.IsRetrying || state.IsRestored);
    }

    [Fact]
    public void ShellSidebarSet_OpensAndClosesTheSidebar_AndItStartsOpen()
    {
        Assert.True(new FeatureShellState().SidebarOpen);

        FeatureShellState state = FeatureShellReducers.ReduceSidebarSet(new(), new(false));
        Assert.False(state.SidebarOpen);

        Assert.True(FeatureShellReducers.ReduceSidebarSet(state, new(true)).SidebarOpen);
    }

    [Fact]
    public void DownloadChanged_ReplacesTheProgress()
    {
        DownloadProgress progress = new(10, 4, 1, 2048, DownloadStopReasonEnum.StorageLow);

        FeatureDownloadState state = FeatureDownloadReducers.ReduceChanged(new(), new(progress));

        Assert.Equal(progress, state.Progress);
        Assert.Equal(0, new FeatureDownloadState().Progress.Total);
    }

    [Fact]
    public void PlayerChanged_ReplacesTheSnapshot_AndTheInitialOneIsDisabled()
    {
        Assert.True(new FeaturePlayerState().Snapshot.IsDisabled);
        Assert.Null(new FeaturePlayerState().Snapshot.Current);

        PlayerSnapshot snapshot = new(new(Guid.NewGuid(), Guid.NewGuid()), false, true, false, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(200), 0.5, ShuffleModeEnum.Random, RepeatModeEnum.Once, Guid.NewGuid(), 12);

        FeaturePlayerState state = FeaturePlayerReducers.ReduceChanged(new(), new(snapshot));

        Assert.Equal(snapshot, state.Snapshot);
        Assert.NotNull(state.Snapshot.Current);
        Assert.IsType<QueueItem>(state.Snapshot.Current);
    }
}
