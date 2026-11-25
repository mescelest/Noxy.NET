using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Presentation.Enums;
using Noxy.NET.MediaPlayer.Presentation.Features.Download;
using Noxy.NET.MediaPlayer.Presentation.Features.Library;
using Noxy.NET.MediaPlayer.Presentation.Features.MetadataList;
using Noxy.NET.MediaPlayer.Presentation.Features.Player;
using Noxy.NET.MediaPlayer.Presentation.Features.PlaylistList;
using Noxy.NET.MediaPlayer.Presentation.Features.Session;
using Noxy.NET.MediaPlayer.Presentation.Models;
using Noxy.NET.MediaPlayer.Presentation.Services;
using Noxy.NET.Test.MediaPlayer.Fakes;
using Noxy.NET.Test.MediaPlayer.Fixtures;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class StateBridgeServiceTest
{
    private readonly FakeDispatcher _dispatcher = new();
    private readonly FakeSyncService _sync = new();
    private readonly FakeSessionService _session = new();
    private readonly FakeDownloadService _download = new();
    private readonly FakePlayerService _player = new();
    private readonly FakeAppStartupService _startup = new();
    private readonly FakeLocalDataOwnerService _owner = new();
    private readonly FakeMetadataStore _storeMetadata = new();
    private readonly FakeMetadataSyncService _syncMetadata = new();
    private readonly StateBridgeService _bridge;

    public StateBridgeServiceTest()
    {
        _bridge = new(_dispatcher, _sync, _session, _download, _player, _startup, _owner, _storeMetadata, _storeMetadata, _syncMetadata, new FakeLogger<StateBridgeService>());
    }

    private static async Task WaitUntil(Func<bool> condition)
    {
        DateTime deadline = DateTime.UtcNow.AddSeconds(10);
        while (!condition())
        {
            Assert.True(DateTime.UtcNow < deadline, "Timed out waiting for the condition.");
            await Task.Delay(10, TestContext.Current.CancellationToken);
        }
    }

    [Fact]
    public async Task Start_DispatchesTheCurrentSessionLibraryPlaylistDownloadAndPlayerState()
    {
        ResponseMediaFile media = MediaFileFactory.Create("One");
        await _storeMetadata.SaveMediaFile(media);
        await _storeMetadata.SaveFileState(new(media.ID, FileSyncStateEnum.OnDevice, "hash", null));
        _download.Progress = new(3, 1, 0, 100, null);
        _sync.IsOffline = true;

        _bridge.Start();
        await WaitUntil(() => _dispatcher.Of<ActionLibraryStateLoaded>().Count == 1);

        ActionSessionChanged session = Assert.Single(_dispatcher.Of<ActionSessionChanged>());
        Assert.True(session.IsSignedIn);
        Assert.True(session.IsOffline);
        Assert.Equal([media.ID], Assert.Single(_dispatcher.Of<ActionLibraryMediaLoaded>()).MediaList.Select(x => x.ID));
        Assert.Empty(Assert.Single(_dispatcher.Of<ActionPlaylistListLoaded>()).PlaylistList);
        Assert.Equal(3, Assert.Single(_dispatcher.Of<ActionDownloadChanged>()).Progress.Total);
        Assert.True(Assert.Single(_dispatcher.Of<ActionPlayerChanged>()).Snapshot.IsDisabled);
        Assert.Equal(media.ID, Assert.Single(Assert.Single(_dispatcher.Of<ActionLibraryStateLoaded>()).StateList).MediaFileID);
    }

    [Fact]
    public async Task SyncChanged_ReloadsTheMediaAndPlaylistLists_NotTheFileStates()
    {
        _bridge.Start();
        await WaitUntil(() => _dispatcher.Of<ActionLibraryStateLoaded>().Count == 1);
        await _storeMetadata.SaveMediaFile(MediaFileFactory.Create("New"));
        _dispatcher.Clear();
        int stateReads = _storeMetadata.FileStateListReadCount;

        _sync.RaiseChanged();
        await WaitUntil(() => _dispatcher.Of<ActionPlaylistListLoaded>().Count == 1);

        Assert.Single(Assert.Single(_dispatcher.Of<ActionLibraryMediaLoaded>()).MediaList);
        Assert.Empty(_dispatcher.Of<ActionLibraryStateLoaded>());
        Assert.Equal(stateReads, _storeMetadata.FileStateListReadCount);
    }

    [Fact]
    public async Task DownloadChanged_ReloadsTheFileStatesAndTheProgress_NotTheMediaList()
    {
        _bridge.Start();
        await WaitUntil(() => _dispatcher.Of<ActionLibraryStateLoaded>().Count == 1);
        _dispatcher.Clear();
        int mediaReads = _storeMetadata.MediaFileListReadCount;
        _download.Progress = new(5, 2, 0, 9, null);

        _download.RaiseChanged();
        await WaitUntil(() => _dispatcher.Of<ActionLibraryStateLoaded>().Count == 1);

        Assert.Equal(5, Assert.Single(_dispatcher.Of<ActionDownloadChanged>()).Progress.Total);
        Assert.Equal(mediaReads, _storeMetadata.MediaFileListReadCount);
        Assert.Empty(_dispatcher.Of<ActionLibraryMediaLoaded>());
    }

    [Fact]
    public async Task PlayerChanged_DispatchesAFreshSnapshot()
    {
        _bridge.Start();
        await WaitUntil(() => _dispatcher.Of<ActionLibraryStateLoaded>().Count == 1);
        _dispatcher.Clear();
        _player.Current = new(Guid.NewGuid(), Guid.NewGuid());
        _player.IsPlaying = true;
        _player.Position = TimeSpan.FromSeconds(12);

        _player.RaiseChanged();

        PlayerSnapshot snapshot = Assert.Single(_dispatcher.Of<ActionPlayerChanged>()).Snapshot;
        Assert.False(snapshot.IsDisabled);
        Assert.True(snapshot.IsPlaying);
        Assert.Equal(TimeSpan.FromSeconds(12), snapshot.Position);
        Assert.Equal(_player.Current, snapshot.Current);
    }

    [Fact]
    public async Task ASignedInSession_ShowsNoSessionToTheScreens_UntilTheOwnerIsVerified()
    {
        _owner.IsVerified = false;
        _bridge.Start();
        await WaitUntil(() => _dispatcher.Of<ActionLibraryStateLoaded>().Count == 1);
        Assert.False(_dispatcher.Of<ActionSessionChanged>().Last().HasSession);
        Assert.True(_dispatcher.Of<ActionSessionChanged>().Last().IsRestored);

        _owner.IsVerified = true;
        _owner.RaiseChanged();

        Assert.True(_dispatcher.Of<ActionSessionChanged>().Last().HasSession);
        Assert.True(_dispatcher.Of<ActionSessionChanged>().Last().IsSignedIn);
    }

    [Fact]
    public async Task AnOfflineSession_ShowsTheLocalData_WithoutAnOwnerCheck()
    {
        _owner.IsVerified = false;
        _session.AccessToken = null;
        _session.RestoreResult = RestoreResultEnum.Offline;

        _bridge.Start();
        await WaitUntil(() => _dispatcher.Of<ActionLibraryStateLoaded>().Count == 1);

        Assert.True(_dispatcher.Of<ActionSessionChanged>().Last().HasSession);
    }

    [Fact]
    public async Task SessionChanged_DispatchesTheSessionState()
    {
        _bridge.Start();
        await WaitUntil(() => _dispatcher.Of<ActionLibraryStateLoaded>().Count == 1);
        _dispatcher.Clear();
        _session.AccessToken = null;

        _session.RaiseChanged();

        ActionSessionChanged session = Assert.Single(_dispatcher.Of<ActionSessionChanged>());
        Assert.False(session.IsSignedIn);
        Assert.False(session.HasSession);
    }

    [Fact]
    public async Task ABurstOfChanges_RefreshesAtMostOnceWhileOneRefreshIsRunning_ThenOnceMore()
    {
        TaskCompletionSource gate = new(TaskCreationOptions.RunContinuationsAsynchronously);
        _storeMetadata.MediaFileListGate = gate.Task;
        _bridge.Start();
        await WaitUntil(() => _storeMetadata.MediaFileListReadCount == 1);

        for (int change = 0; change < 10; change++) _sync.RaiseChanged();

        Assert.Equal(1, _storeMetadata.MediaFileListReadCount);

        gate.SetResult();
        await WaitUntil(() => _storeMetadata.MediaFileListReadCount == 2);
        await Task.Delay(50, TestContext.Current.CancellationToken);

        Assert.Equal(2, _storeMetadata.MediaFileListReadCount);
        Assert.Equal(2, _dispatcher.Of<ActionLibraryMediaLoaded>().Count);
    }

    [Fact]
    public async Task AFailedRead_IsLoggedByTypeOnly_AndTheNextChangeReadsAgain()
    {
        FakeLogger<StateBridgeService> logger = new();
        _storeMetadata.MediaFileListGate = Task.FromException(new InvalidOperationException("secret detail"));
        StateBridgeService bridge = new(_dispatcher, _sync, _session, _download, _player, _startup, _owner, _storeMetadata, _storeMetadata, _syncMetadata, logger);
        bridge.Start();

        _sync.RaiseChanged();
        await WaitUntil(() => logger.EntryList.Count >= 2);

        Assert.All(logger.EntryList, x => Assert.DoesNotContain("secret", x.Message));
        Assert.Contains(logger.EntryList, x => x.Message.Contains(nameof(InvalidOperationException)));
    }

    [Fact]
    public async Task Start_CalledTwice_SubscribesOnce()
    {
        _bridge.Start();
        _bridge.Start();
        await WaitUntil(() => _dispatcher.Of<ActionLibraryStateLoaded>().Count == 1);
        _dispatcher.Clear();

        _player.RaiseChanged();

        Assert.Single(_dispatcher.Of<ActionPlayerChanged>());
    }

    [Fact]
    public async Task AfterDispose_TheBridgeNoLongerListens_AndAStartedOneStaysStopped()
    {
        _bridge.Start();
        await WaitUntil(() => _dispatcher.Of<ActionLibraryStateLoaded>().Count == 1);
        _bridge.Dispose();
        _dispatcher.Clear();

        _player.RaiseChanged();
        _sync.RaiseChanged();
        _download.RaiseChanged();
        _session.RaiseChanged();
        _bridge.Start();
        await Task.Delay(50, TestContext.Current.CancellationToken);

        Assert.Empty(_dispatcher.ActionList);
    }

    [Fact]
    public async Task WhenTheLocalDataWasWiped_EverythingIsReadAgain()
    {
        _bridge.Start();
        _dispatcher.ActionList.Clear();

        _owner.RaiseWiped();

        await WaitUntil(() => _dispatcher.Of<ActionPlaylistListLoaded>().Count >= 1 && _dispatcher.Of<ActionLibraryMediaLoaded>().Count >= 1);
    }

    [Fact]
    public async Task Refresh_ReadsEverythingAgain()
    {
        await _bridge.Refresh();

        Assert.Single(_dispatcher.Of<ActionLibraryMediaLoaded>());
        Assert.Single(_dispatcher.Of<ActionLibraryStateLoaded>());
        Assert.Single(_dispatcher.Of<ActionPlaylistListLoaded>());
        Assert.Single(_dispatcher.Of<ActionSessionChanged>());
        Assert.Single(_dispatcher.Of<ActionDownloadChanged>());
        Assert.Single(_dispatcher.Of<ActionPlayerChanged>());
    }

    [Fact]
    public async Task AChangeRaisedByTheMetadataSyncService_DispatchesTheMetadataListsAsStored()
    {
        _bridge.Start();
        await WaitUntil(() => _dispatcher.Of<ActionLibraryStateLoaded>().Count == 1);
        Guid artistID = Guid.CreateVersion7();
        await _storeMetadata.Artist.Save(FakeApiClient.ArtistFor(artistID, "Artist"));
        _dispatcher.Clear();

        _syncMetadata.RaiseChanged();
        await WaitUntil(() => _dispatcher.Of<ActionMetadataListLoaded>().Count == 1);

        ActionMetadataListLoaded loaded = Assert.Single(_dispatcher.Of<ActionMetadataListLoaded>());
        Assert.Equal(artistID, Assert.Single(loaded.ArtistList).ID);
        Assert.Empty(loaded.AlbumList);
        Assert.Empty(loaded.GenreList);
    }

    [Fact]
    public async Task Start_DispatchesTheMetadataLists()
    {
        _bridge.Start();

        await WaitUntil(() => _dispatcher.Of<ActionMetadataListLoaded>().Count == 1);
    }
}
