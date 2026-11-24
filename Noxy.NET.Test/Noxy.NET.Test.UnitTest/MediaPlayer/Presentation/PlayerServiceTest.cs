using Microsoft.Extensions.Logging;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Models;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;
using Noxy.NET.MediaPlayer.Domain.Responses.Remote;
using Noxy.NET.MediaPlayer.Presentation;
using Noxy.NET.MediaPlayer.Presentation.Enums;
using Noxy.NET.MediaPlayer.Presentation.Services;
using Noxy.NET.Test.MediaPlayer.Fakes;
using Noxy.NET.Test.MediaPlayer.Fixtures;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class PlayerServiceTest
{
    private readonly FakeAudioPlayer _audio = new();
    private readonly FakeDownloadService _download = new();
    private readonly FakeFileStore _storeFile = new();
    private readonly FakeMetadataStore _storeMetadata = new();
    private readonly FakeSyncService _sync = new();
    private readonly FakeSettingService _serviceSetting = new();
    private readonly FakeApiClient _api = new();
    private readonly FakeDelay _delay = new();
    private readonly FakeLocalQueueStore _storeLocalQueue = new();
    private readonly FakeLogger<PlayerService> _logger = new();
    private readonly QueueService _queue;
    private readonly PlayerService _player;
    private int _titleCount;

    public PlayerServiceTest()
    {
        QueueSourceService serviceSource = new(_storeMetadata, new ShuffleService(new(31)), new(32));
        _queue = new(serviceSource, _serviceSetting, _storeLocalQueue, _sync, _storeMetadata, new FakeTimeProvider(), new FakeLogger<QueueService>()) { OwnsPlayback = true };
        _player = new(_audio, _download, _storeFile, _storeMetadata, _sync, _storeMetadata, _queue, serviceSource, _serviceSetting, _api, _delay, _logger);
    }

    // A media file in the library, titled so that the Library's order is the order of creation; its bytes are on the device unless said otherwise.
    private async Task<Guid> AddMedia(bool onDevice = true)
    {
        ResponseMediaFile media = MediaFileFactory.Create($"Media {_titleCount++:000}");
        await _storeMetadata.SaveMediaFile(media);
        if (onDevice) _storeFile.Hold(media.ID, [1, 2, 3]);

        return media.ID;
    }

    private async Task<List<Guid>> AddMedia(int count, bool onDevice = true)
    {
        List<Guid> idList = [];
        for (int i = 0; i < count; i++) idList.Add(await AddMedia(onDevice));

        return idList;
    }

    private async Task Start(Guid mediaFileID)
    {
        await _player.StartLibrary(mediaFileID, LibrarySortEnum.Title, false, string.Empty);
    }

    private async Task<(Guid PlaylistID, List<ResponsePlaylistEntry> EntryList)> AddPlaylist(List<Guid> mediaFileIDList)
    {
        Guid playlistID = Guid.NewGuid();
        List<ResponsePlaylistEntry> entryList = [];
        foreach (Guid mediaFileID in mediaFileIDList)
        {
            ResponseMediaFile media = await _storeMetadata.GetMediaFile(mediaFileID) ?? throw new InvalidOperationException("Missing media.");
            entryList.Add(new() { ID = Guid.NewGuid(), MediaFileID = mediaFileID, Position = entryList.Count, MediaFile = media });
        }

        ResponsePlaylistDetail detail = new()
        {
            ID = playlistID, Name = "List", AllowDuplicates = true, EntryCount = entryList.Count, TimeCreated = DateTime.UtcNow, TimeModified = DateTime.UtcNow, TimeLastPlayed = null, EntryList = entryList
        };
        await _storeMetadata.SavePlaylist(detail);
        await _storeMetadata.SavePlaylistDetail(detail);

        return (playlistID, entryList);
    }

    // A download that puts the file in the store and succeeds.
    private void DownloadsSucceed()
    {
        _download.EnsureHandler = id =>
        {
            _storeFile.Hold(id, [1, 2, 3]);
            return Task.FromResult(true);
        };
    }

    private async Task<string> PathOf(Guid id)
    {
        return await _storeFile.GetPath(id) ?? throw new InvalidOperationException("The test media file is not held.");
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

    private bool IsPlayingMedia(Guid mediaFileID)
    {
        return _player.Current?.MediaFileID == mediaFileID && _audio.IsPlaying;
    }

    private List<string> LoadList => _audio.CallList.Where(x => x.StartsWith("Load ")).ToList();

    private ResponseQueue Snapshot()
    {
        return _player.Queue.ToResponse();
    }

    private static List<Guid> MediaOf(List<QueueItem> itemList)
    {
        return itemList.Select(x => x.MediaFileID).ToList();
    }

    private async Task AddToQueue(Guid mediaFileID)
    {
        await _queue.Apply(new() { Edit = QueueEditEnum.AddToQueue, ItemID = Guid.NewGuid(), MediaFileID = mediaFileID });
    }

    private async Task PlayNext(Guid mediaFileID)
    {
        await _queue.Apply(new() { Edit = QueueEditEnum.PlayNext, ItemID = Guid.NewGuid(), MediaFileID = mediaFileID });
    }

    private static string Json(Guid id)
    {
        return id.ToString();
    }

    #region -- Starting --

    [Fact]
    public async Task StartLibrary_WhenOnDevice_LoadsTheStoredPath_AndPlays_WithNoDownload()
    {
        Guid id = await AddMedia();

        await Start(id);

        Assert.Equal([$"Load {await PathOf(id)}"], LoadList);
        Assert.Contains("Play", _audio.CallList);
        Assert.Empty(_download.EnsureList);
        Assert.Equal(id, _player.Current?.MediaFileID);
        Assert.True(_audio.IsPlaying);
    }

    [Fact]
    public async Task StartLibrary_WhenMissing_DownloadsFirst_ThenPlays_AndIsWaitingInBetween()
    {
        Guid id = await AddMedia(false);
        TaskCompletionSource gate = new(TaskCreationOptions.RunContinuationsAsynchronously);
        _download.EnsureHandler = async downloaded =>
        {
            await gate.Task;
            _storeFile.Hold(downloaded, [1, 2, 3]);

            return true;
        };

        Task play = Start(id);
        await WaitUntil(() => _player.IsWaiting);

        Assert.Empty(LoadList);
        Assert.False(_audio.IsPlaying);

        gate.SetResult();
        await play;

        Assert.False(_player.IsWaiting);
        Assert.Equal([$"Load {await PathOf(id)}"], LoadList);
        Assert.True(_audio.IsPlaying);
    }

    [Fact]
    public async Task StartLibrary_WhenTheDownloadFails_DoesNotPlay_AndSkipsToTheNextPlayableItem()
    {
        Guid bad = await AddMedia(false);
        Guid good = await AddMedia();
        _download.EnsureResult = false;

        await Start(bad);

        Assert.Equal(good, _player.Current?.MediaFileID);
        Assert.Equal([$"Load {await PathOf(good)}"], LoadList);
        QueueItem failed = Snapshot().HistoryList.Single();
        Assert.Equal(bad, failed.MediaFileID);
        Assert.Equal(FileSyncStateEnum.Failed, _player.StateOf(failed));
    }

    [Fact]
    public async Task StartLibrary_WhenEveryItemFails_PlaysNothing_AndDoesNotThrow()
    {
        List<Guid> idList = await AddMedia(2, false);
        _download.EnsureResult = false;

        await Start(idList[0]);

        Assert.Empty(LoadList);
        Assert.False(_player.IsPlaying);
        ResponseQueue queue = Snapshot();
        Assert.All([.. queue.HistoryList, queue.Current ?? throw new InvalidOperationException("No current.")], x => Assert.Equal(FileSyncStateEnum.Failed, _player.StateOf(x)));
    }

    [Fact]
    public async Task StartLibrary_ForAMediaFileThatIsNotInTheLibrary_DoesNotPlay()
    {
        await _player.StartLibrary(Guid.CreateVersion7(), LibrarySortEnum.Title, false, string.Empty);

        Assert.Empty(LoadList);
        Assert.Empty(_download.EnsureList);
        Assert.True(_player.IsDisabled);
    }

    [Fact]
    public async Task StartLibrary_BuildsTheQueue_AndPlaysTheClickedMediaFile()
    {
        List<Guid> idList = await AddMedia(4);

        await Start(idList[1]);

        ResponseQueue queue = Snapshot();
        Assert.Equal(idList[1], _player.Current?.MediaFileID);
        Assert.Equal([idList[2], idList[3]], MediaOf(queue.SourceList));
        Assert.Equal(QueueSourceKindEnum.Library, queue.Source?.Kind);
        Assert.Null(_player.PlaylistID);
    }

    [Fact]
    public async Task StartLibrary_WithShuffle_PlaysTheClickedMediaFileFirst()
    {
        List<Guid> idList = await AddMedia(12);
        _serviceSetting.Shuffle = ShuffleModeEnum.Random;

        await Start(idList[5]);

        Assert.Equal(idList[5], _player.Current?.MediaFileID);
        Assert.Equal(11, Snapshot().SourceList.Count);
        Assert.Equal(await PathOf(idList[5]), _audio.LoadedPath);
        Assert.Equal(ShuffleModeEnum.Random, _player.Shuffle);
    }

    [Fact]
    public async Task StartLibrary_ReplacesTheQueue()
    {
        List<Guid> idList = await AddMedia(3);
        await Start(idList[0]);

        await Start(idList[2]);

        Assert.Equal(idList[2], _player.Current?.MediaFileID);
        Assert.Empty(Snapshot().SourceList);
        Assert.Empty(Snapshot().HistoryList);
    }

    [Fact]
    public async Task AStartThatIsSupersededWhileItDownloads_NeverLoadsItsFile()
    {
        Guid slow = await AddMedia(false);
        Guid other = await AddMedia();
        TaskCompletionSource gate = new(TaskCreationOptions.RunContinuationsAsynchronously);
        _download.EnsureHandler = async id =>
        {
            await gate.Task;
            _storeFile.Hold(id, [1, 2, 3]);

            return true;
        };
        Task first = Start(slow);
        await WaitUntil(() => _player.IsWaiting);

        await Start(other);
        gate.SetResult();
        await first;

        Assert.Equal([$"Load {await PathOf(other)}"], LoadList);
        Assert.Equal(other, _player.Current?.MediaFileID);
    }

    [Fact]
    public async Task StartPlaylist_PlaysFromTheEntry_AndReportsThePlaylistPlayedOnce()
    {
        List<Guid> idList = await AddMedia(3);
        (Guid playlistID, List<ResponsePlaylistEntry> entryList) = await AddPlaylist(idList);

        await _player.StartPlaylist(playlistID, entryList[1].ID);

        Assert.Equal(entryList[1].ID, _player.Current?.ItemID);
        Assert.Equal([idList[2]], MediaOf(Snapshot().SourceList));
        Assert.Equal([playlistID], _api.PlayedRequestList);
        Assert.Equal(playlistID, _player.PlaylistID);

        _audio.RaiseEnded();
        await WaitUntil(() => IsPlayingMedia(idList[2]));

        Assert.Equal([playlistID], _api.PlayedRequestList);
    }

    [Fact]
    public async Task StartPlaylist_WithNoEntry_StartsAtTheFirstOne()
    {
        List<Guid> idList = await AddMedia(2);
        (Guid playlistID, _) = await AddPlaylist(idList);

        await _player.StartPlaylist(playlistID);

        Assert.Equal(idList[0], _player.Current?.MediaFileID);
    }

    [Fact]
    public async Task StartPlaylist_ForAPlaylistThatCannotBeBuilt_PlaysNothing_AndReportsNothing()
    {
        await _player.StartPlaylist(Guid.NewGuid());

        Assert.True(_player.IsDisabled);
        Assert.Empty(_api.PlayedRequestList);
    }

    [Fact]
    public async Task ReportingPlayed_WhenOffline_IsIgnored_AndPlaybackContinues()
    {
        List<Guid> idList = await AddMedia(2);
        (Guid playlistID, _) = await AddPlaylist(idList);
        _api.MarkPlaylistPlayedException = new HttpRequestException("Offline.");

        await _player.StartPlaylist(playlistID);

        Assert.True(_audio.IsPlaying);
        Assert.Equal(idList[0], _player.Current?.MediaFileID);
    }

    [Fact]
    public async Task PlayCurrent_PlaysTheQueuesCurrentItem()
    {
        Guid id = await AddMedia();
        QueueItem current = new(Guid.NewGuid(), id);
        await _queue.ReplaceWithSnapshot(new() { QueueRevision = 1, Source = null, HistoryList = [], Current = current, UserQueueList = [], SourceList = [] }, false);

        await _player.PlayCurrent();

        Assert.Equal(await PathOf(id), _audio.LoadedPath);
        Assert.True(_audio.IsPlaying);
    }

    [Fact]
    public async Task PlayCurrent_WithNoCurrentItem_DoesNothing()
    {
        await _player.PlayCurrent();

        Assert.Empty(_audio.CallList);
    }

    #endregion -- Starting --

    #region -- Moving through the queue --

    [Fact]
    public async Task Ended_AdvancesToTheNextItem()
    {
        List<Guid> idList = await AddMedia(2);
        await Start(idList[0]);

        _audio.RaiseEnded();
        await WaitUntil(() => IsPlayingMedia(idList[1]));

        Assert.Equal([$"Load {await PathOf(idList[0])}", $"Load {await PathOf(idList[1])}"], LoadList);
        Assert.True(_audio.IsPlaying);
        Assert.Equal(idList[0], Snapshot().HistoryList.Single().MediaFileID);
    }

    [Fact]
    public async Task Ended_AtTheEndOfTheQueue_Stops_AndReleasesTheFile()
    {
        Guid id = await AddMedia();
        await Start(id);

        _audio.RaiseEnded();
        await WaitUntil(() => _audio.LoadedPath is null);

        Assert.False(_audio.IsPlaying);
        Assert.Equal("Stop", _audio.CallList.Last());
    }

    [Fact]
    public async Task Next_MovesToTheNextItem_AndAtTheEnd_DoesNothing()
    {
        List<Guid> idList = await AddMedia(2);
        await Start(idList[0]);

        await _player.Next();
        Assert.Equal(idList[1], _player.Current?.MediaFileID);
        int calls = _audio.CallList.Count;

        await _player.Next();

        Assert.Equal(idList[1], _player.Current?.MediaFileID);
        Assert.Equal(calls, _audio.CallList.Count);
        Assert.True(_audio.IsPlaying);
    }

    [Fact]
    public async Task AFinishedItem_AdvancesToTheUserQueueBeforeTheSourcePart()
    {
        List<Guid> idList = await AddMedia(3);
        Guid queued = await AddMedia();
        await Start(idList[0]);
        await AddToQueue(queued);

        _audio.RaiseEnded();
        await WaitUntil(() => IsPlayingMedia(queued));

        _audio.RaiseEnded();
        await WaitUntil(() => IsPlayingMedia(idList[1]));
    }

    [Fact]
    public async Task AUserQueueItem_PlaysOnceAndIsConsumed()
    {
        List<Guid> idList = await AddMedia(2);
        await Start(idList[0]);
        await PlayNext(idList[0]);

        _audio.RaiseEnded();
        await WaitUntil(() => LoadList.Count == 2);

        Assert.Empty(Snapshot().UserQueueList);
        Assert.Equal(LoadList[0], LoadList[1]);

        _audio.RaiseEnded();
        await WaitUntil(() => IsPlayingMedia(idList[1]));
    }

    [Fact]
    public async Task JumpTo_AnItemAhead_PlaysIt_AndDropsTheSkippedOnes_OrKeepsThemInHistory()
    {
        List<Guid> idList = await AddMedia(5);
        await Start(idList[0]);
        QueueItem target = Snapshot().SourceList[2];

        await _player.JumpTo(target.ItemID, false);

        ResponseQueue dropped = Snapshot();
        Assert.Equal(target, dropped.Current);
        Assert.Equal([idList[0]], MediaOf(dropped.HistoryList));
        Assert.Equal(await PathOf(idList[3]), _audio.LoadedPath);

        await Start(idList[0]);
        await _player.JumpTo(Snapshot().SourceList[2].ItemID, true);

        Assert.Equal([idList[0], idList[1], idList[2]], MediaOf(Snapshot().HistoryList));
    }

    [Fact]
    public async Task JumpTo_AnItemBehind_StepsBackToIt_AndAnItemThatIsGone_DoesNothing()
    {
        List<Guid> idList = await AddMedia(4);
        await Start(idList[0]);
        await _player.Next();
        await _player.Next();
        QueueItem first = Snapshot().HistoryList[0];
        int loads = LoadList.Count;

        await _player.JumpTo(Guid.NewGuid(), false);
        Assert.Equal(loads, LoadList.Count);

        await _player.JumpTo(first.ItemID, false);

        Assert.Equal(first, _player.Current);
        Assert.Equal(await PathOf(idList[0]), _audio.LoadedPath);
        Assert.Empty(Snapshot().HistoryList);
    }

    [Fact]
    public async Task Previous_GoesToTheLastHistoryItem()
    {
        List<Guid> idList = await AddMedia(3);
        await Start(idList[0]);
        await _player.Next();
        await _player.Next();

        await _player.Previous();

        Assert.Equal(idList[1], _player.Current?.MediaFileID);
        Assert.Equal([idList[0]], MediaOf(Snapshot().HistoryList));
        Assert.Equal([idList[2]], MediaOf(Snapshot().SourceList));
    }

    [Fact]
    public async Task Previous_AtTheStart_RestartsTheCurrentMediaFile()
    {
        Guid id = await AddMedia();
        await Start(id);
        _audio.CallList.Clear();

        await _player.Previous();

        Assert.Equal(id, _player.Current?.MediaFileID);
        Assert.Contains($"Seek {TimeSpan.Zero}", _audio.CallList);
        Assert.DoesNotContain(_audio.CallList, x => x.StartsWith("Load "));
    }

    [Fact]
    public async Task Previous_PastThreeSeconds_RestartsTheCurrentItem_InsteadOfGoingBack()
    {
        List<Guid> idList = await AddMedia(2);
        await Start(idList[0]);
        await _player.Next();
        _audio.Position = TimeSpan.FromSeconds(10);
        _audio.CallList.Clear();

        await _player.Previous();

        Assert.Equal(idList[1], _player.Current?.MediaFileID);
        Assert.Contains($"Seek {TimeSpan.Zero}", _audio.CallList);
        Assert.True(_player.PositionJumped);
    }

    [Theory]
    [InlineData(ShuffleModeEnum.Off)]
    [InlineData(ShuffleModeEnum.Random)]
    public async Task Previous_AfterADeletedItem_GoesToTheItemBeforeIt(ShuffleModeEnum shuffle)
    {
        List<Guid> idList = await AddMedia(5);
        _serviceSetting.Shuffle = shuffle;
        await Start(idList[0]);
        ResponseQueue start = Snapshot();
        Guid first = start.Current?.MediaFileID ?? throw new InvalidOperationException("No current.");
        Guid second = start.SourceList[0].MediaFileID;
        Guid third = start.SourceList[1].MediaFileID;
        await _player.Next();

        _sync.RaiseLibraryDiff(new([], [], [second]));
        await _player.Next();

        Assert.Equal(third, _player.Current?.MediaFileID);
        Assert.Equal([first], MediaOf(Snapshot().HistoryList));

        await _player.Previous();

        Assert.Equal(first, _player.Current?.MediaFileID);
    }

    [Fact]
    public async Task PreviousWhileADeletedMediaFileIsStillPlaying_RestartsIt()
    {
        List<Guid> idList = await AddMedia(2);
        await Start(idList[0]);
        await _player.Next();
        await _storeMetadata.DeleteMediaFile(idList[1]);
        _sync.RaiseLibraryDiff(new([], [], [idList[1]]));
        _audio.CallList.Clear();

        await _player.Previous();

        Assert.Equal(idList[1], _player.Current?.MediaFileID);
        Assert.Equal([idList[0]], MediaOf(Snapshot().HistoryList));
        Assert.Contains($"Seek {TimeSpan.Zero}", _audio.CallList);
        Assert.DoesNotContain(_audio.CallList, x => x.StartsWith("Load "));
    }

    [Fact]
    public async Task ARemovedCurrentItem_SkipsToTheNextOne_AndNeverEntersHistory()
    {
        List<Guid> idList = await AddMedia(3);
        await Start(idList[0]);
        QueueItem current = _player.Current ?? throw new InvalidOperationException("No current.");

        await _queue.Apply(new() { Edit = QueueEditEnum.Remove, ItemID = current.ItemID });
        await WaitUntil(() => IsPlayingMedia(idList[1]));

        Assert.Empty(Snapshot().HistoryList);
        Assert.Equal(await PathOf(idList[1]), _audio.LoadedPath);
    }

    [Fact]
    public async Task Clear_StopsAndEmptiesTheQueue()
    {
        List<Guid> idList = await AddMedia(2);
        await Start(idList[0]);

        await _player.Clear();

        Assert.Equal(0, _player.Queue.ItemCount);
        Assert.Null(_player.Current);
        Assert.Equal("Stop", _audio.CallList.Last());
        Assert.False(_player.IsPlaying);
    }

    [Fact]
    public async Task TheAudioPlayer_IsStoppedBeforeEveryLoad_SoThePreviousFileIsNeverHeldOpen()
    {
        List<Guid> idList = await AddMedia(3);
        await Start(idList[0]);
        await _player.Next();
        await _player.Next();

        for (int i = 0; i < _audio.CallList.Count; i++)
        {
            if (_audio.CallList[i].StartsWith("Load ")) Assert.Equal("Stop", _audio.CallList[i - 1]);
        }

        Assert.Equal(3, LoadList.Count);
    }

    #endregion -- Moving through the queue --

    #region -- Repeat --

    [Fact]
    public async Task RepeatNo_StopsAtTheEndOfTheSourcePart()
    {
        _serviceSetting.Repeat = RepeatModeEnum.No;
        List<Guid> idList = await AddMedia(2);
        await Start(idList[0]);

        _audio.RaiseEnded();
        await WaitUntil(() => IsPlayingMedia(idList[1]));
        _audio.RaiseEnded();
        await WaitUntil(() => _player.IsDisabled && _audio.LoadedPath is null);

        Assert.Equal(2, LoadList.Count);
        Assert.True(_player.IsDisabled);
    }

    [Fact]
    public async Task RepeatOnce_AppendsTheSourceOnce_ThenStops()
    {
        _serviceSetting.Repeat = RepeatModeEnum.Once;
        List<Guid> idList = await AddMedia(2);
        await Start(idList[0]);

        for (int round = 1; round <= 3; round++)
        {
            int loads = LoadList.Count;
            _audio.RaiseEnded();
            await WaitUntil(() => LoadList.Count == loads + 1 && _audio.IsPlaying);
        }

        _audio.RaiseEnded();
        await WaitUntil(() => _player.IsDisabled);

        Assert.Equal(4, LoadList.Count);
    }

    [Fact]
    public async Task RepeatForever_AppendsTheSourceAgainEachTimeItEnds()
    {
        _serviceSetting.Repeat = RepeatModeEnum.Forever;
        List<Guid> idList = await AddMedia(2);
        await Start(idList[0]);

        for (int round = 1; round <= 6; round++)
        {
            int loads = LoadList.Count;
            _audio.RaiseEnded();
            await WaitUntil(() => LoadList.Count == loads + 1);
        }

        Assert.Equal(7, LoadList.Count);
        Assert.False(_player.IsDisabled);
    }

    [Fact]
    public async Task ARepeatedPass_IsReshuffled()
    {
        _serviceSetting.Repeat = RepeatModeEnum.Once;
        _serviceSetting.Shuffle = ShuffleModeEnum.Random;
        List<Guid> idList = await AddMedia(30);
        await Start(idList[0]);
        for (int played = 0; played < 29; played++) await _player.Next();
        List<Guid> firstPass = [.. MediaOf(Snapshot().HistoryList), _player.Current?.MediaFileID ?? Guid.Empty];

        await _player.Next();

        ResponseQueue queue = Snapshot();
        List<Guid> secondPass = [queue.Current?.MediaFileID ?? Guid.Empty, .. MediaOf(queue.SourceList)];
        Assert.Equal(30, secondPass.Count);
        Assert.Equal(firstPass.Order(), secondPass.Order());
        Assert.NotEqual(firstPass.TakeLast(30).ToList(), secondPass);
    }

    [Fact]
    public async Task RepeatOnAnEmptiedSource_EndsInsteadOfLooping()
    {
        _serviceSetting.Repeat = RepeatModeEnum.Forever;
        Guid id = await AddMedia();
        await Start(id);
        await _storeMetadata.ReplaceMediaFileList([]);

        _audio.RaiseEnded();
        await WaitUntil(() => _player.IsDisabled && _audio.LoadedPath is null);

        Assert.Single(LoadList);
        Assert.True(_player.IsDisabled);
    }

    #endregion -- Repeat --

    #region -- Shuffle and repeat modes --

    [Fact]
    public async Task SetShuffle_OnAPlaylistQueue_WritesTheGlobalSetting_AndRegenerates()
    {
        List<Guid> idList = await AddMedia(10);
        (Guid playlistID, List<ResponsePlaylistEntry> entryList) = await AddPlaylist(idList);
        _serviceSetting.UpdateHandler = (_, _) =>
        {
            _serviceSetting.Shuffle = ShuffleModeEnum.Random;
            return Task.CompletedTask;
        };
        await _player.StartPlaylist(playlistID, entryList[0].ID);
        List<QueueItem> before = Snapshot().SourceList;

        await _player.SetShuffle(ShuffleModeEnum.Random);
        await WaitUntil(() => _serviceSetting.UpdateRequestList.Count == 1);

        Assert.Equal("Shuffle", _serviceSetting.UpdateRequestList.Single().Key);
        Assert.Equal("Random", _serviceSetting.UpdateRequestList.Single().Value.GetString());
        Assert.Empty(_api.UpdatePlaylistRequestList);
        Assert.Equal(before.Select(x => x.ItemID).Order(), Snapshot().SourceList.Select(x => x.ItemID).Order());
        Assert.NotEqual(before, Snapshot().SourceList);
    }

    [Fact]
    public async Task SetShuffle_OnALibraryQueue_WritesTheGlobalSetting()
    {
        List<Guid> idList = await AddMedia(3);
        _serviceSetting.UpdateHandler = (_, _) =>
        {
            _serviceSetting.Shuffle = ShuffleModeEnum.SpreadArtists;
            return Task.CompletedTask;
        };
        await Start(idList[0]);

        await _player.SetShuffle(ShuffleModeEnum.SpreadArtists);
        await WaitUntil(() => _serviceSetting.UpdateRequestList.Count == 1);

        Assert.Equal("Shuffle", _serviceSetting.UpdateRequestList.Single().Key);
        Assert.Equal("SpreadArtists", _serviceSetting.UpdateRequestList.Single().Value.GetString());
        Assert.Empty(_api.UpdatePlaylistRequestList);
        Assert.Equal(ShuffleModeEnum.SpreadArtists, _player.Shuffle);
    }

    [Fact]
    public async Task SetRepeat_WritesTheGlobalSetting_ForALibraryQueueAndAPlaylistQueue()
    {
        List<Guid> idList = await AddMedia(3);
        (Guid playlistID, List<ResponsePlaylistEntry> entryList) = await AddPlaylist(idList);
        await Start(idList[0]);

        await _player.SetRepeat(RepeatModeEnum.Forever);
        await WaitUntil(() => _serviceSetting.UpdateRequestList.Count == 1);
        await _player.StartPlaylist(playlistID, entryList[0].ID);
        await _player.SetRepeat(RepeatModeEnum.Once);
        await WaitUntil(() => _serviceSetting.UpdateRequestList.Count == 2);

        Assert.Equal(["Repeat", "Repeat"], _serviceSetting.UpdateRequestList.Select(x => x.Key));
        Assert.Equal(["Forever", "Once"], _serviceSetting.UpdateRequestList.Select(x => x.Value.GetString()));
        Assert.Empty(_api.UpdatePlaylistRequestList);
    }

    [Fact]
    public async Task AModeChange_ShowsAtOnce_BeforeTheServerHasHeardOfIt()
    {
        List<Guid> idList = await AddMedia(4);
        await Start(idList[0]);
        _delay.IsManual = true;

        await _player.SetShuffle(ShuffleModeEnum.Random);
        await _player.SetRepeat(RepeatModeEnum.Forever);

        Assert.Equal(ShuffleModeEnum.Random, _player.Shuffle);
        Assert.Equal(RepeatModeEnum.Forever, _player.Repeat);
        Assert.Empty(_serviceSetting.UpdateRequestList);

        // Something else reading the stored modes in the meantime does not take the change back.
        _serviceSetting.RaiseChanged();
        await Task.Delay(50, TestContext.Current.CancellationToken);
        Assert.Equal(ShuffleModeEnum.Random, _player.Shuffle);
        Assert.Equal(RepeatModeEnum.Forever, _player.Repeat);
    }

    [Fact]
    public async Task ChangesInARow_SendOnlyTheLastOne_AfterThePause()
    {
        List<Guid> idList = await AddMedia(4);
        await Start(idList[0]);
        _delay.IsManual = true;

        await _player.SetRepeat(RepeatModeEnum.Forever);
        await _player.SetRepeat(RepeatModeEnum.Once);
        await _player.SetRepeat(RepeatModeEnum.No);
        await WaitUntil(() => _delay.PendingCount == 1);
        _delay.ReleaseAll();
        await WaitUntil(() => _serviceSetting.UpdateRequestList.Count == 1);
        await Task.Delay(50, TestContext.Current.CancellationToken);

        Assert.Equal("No", _serviceSetting.UpdateRequestList.Single().Value.GetString());
        Assert.Equal(PresentationConstants.ModeSaveMilliseconds, _delay.DelayList.Last().TotalMilliseconds);
    }

    [Fact]
    public async Task AWriteThatFails_ShowsTheStoredModeAgain()
    {
        List<Guid> idList = await AddMedia(4);
        await Start(idList[0]);
        _serviceSetting.UpdateHandler = (_, _) => throw new HttpRequestException("Offline.");

        await _player.SetShuffle(ShuffleModeEnum.Random);
        await _player.SetRepeat(RepeatModeEnum.Forever);

        await WaitUntil(() => _player.Shuffle == ShuffleModeEnum.Off && _player.Repeat == RepeatModeEnum.No);
    }

    [Fact]
    public async Task TogglingShuffle_NeverReplaysWhatAlreadyPlayed_AndKeepsTheUserQueue()
    {
        List<Guid> idList = await AddMedia(12);
        Guid queued = await AddMedia();
        _serviceSetting.UpdateHandler = (_, _) =>
        {
            _serviceSetting.Shuffle = ShuffleModeEnum.Random;
            return Task.CompletedTask;
        };
        await Start(idList[0]);
        for (int played = 0; played < 3; played++) await _player.Next();
        await PlayNext(queued);
        ResponseQueue before = Snapshot();

        await _player.SetShuffle(ShuffleModeEnum.Random);

        ResponseQueue after = Snapshot();
        Assert.Equal(before.UserQueueList, after.UserQueueList);
        Assert.Equal(before.Current, after.Current);
        Assert.Equal(before.HistoryList, after.HistoryList);
        Assert.DoesNotContain(after.SourceList, x => MediaOf(before.HistoryList).Contains(x.MediaFileID) || x.MediaFileID == before.Current?.MediaFileID);
        Assert.Equal(before.SourceList.Select(x => x.MediaFileID).Where(x => x != queued).Order(), after.SourceList.Select(x => x.MediaFileID).Order());
    }

    [Fact]
    public async Task TheEffectiveMode_IsTheGlobalSetting_ForAPlaylistQueueAndALibraryQueue()
    {
        _serviceSetting.Shuffle = ShuffleModeEnum.SpreadArtists;
        _serviceSetting.Repeat = RepeatModeEnum.Forever;
        List<Guid> idList = await AddMedia(3);
        (Guid playlistID, List<ResponsePlaylistEntry> entryList) = await AddPlaylist(idList);

        await _player.StartPlaylist(playlistID, entryList[0].ID);
        Assert.Equal(ShuffleModeEnum.SpreadArtists, _player.Shuffle);
        Assert.Equal(RepeatModeEnum.Forever, _player.Repeat);

        await Start(idList[0]);
        Assert.Equal(ShuffleModeEnum.SpreadArtists, _player.Shuffle);
        Assert.Equal(RepeatModeEnum.Forever, _player.Repeat);
    }

    #endregion -- Shuffle and repeat modes --

    #region -- Failures --

    [Fact]
    public async Task Failed_FromTheAudioPlayer_MarksTheItemFailed_AndMovesOn_WithoutDeletingTheVerifiedFile()
    {
        List<Guid> idList = await AddMedia(2);
        await Start(idList[0]);
        QueueItem first = _player.Current ?? throw new InvalidOperationException("No current.");

        _audio.RaiseFailed("The file could not be played.");
        await WaitUntil(() => IsPlayingMedia(idList[1]));

        Assert.Equal(FileSyncStateEnum.Failed, _player.StateOf(first));
        Assert.NotNull(await _storeFile.GetPath(idList[0]));
        Assert.DoesNotContain(_audio.CallList, x => x.StartsWith("Delete"));
    }

    [Fact]
    public async Task Failed_OnTheLastItem_StopsWithoutThrowing()
    {
        Guid id = await AddMedia();
        await Start(id);
        QueueItem item = _player.Current ?? throw new InvalidOperationException("No current.");

        _audio.RaiseFailed("The file could not be played.");
        await WaitUntil(() => _player.StateOf(item) == FileSyncStateEnum.Failed && _audio.LoadedPath is null);

        Assert.False(_player.IsPlaying);
        Assert.NotNull(await _storeFile.GetPath(id));
    }

    [Fact]
    public async Task AFileTheAudioPlayerCannotLoad_FailsTheItem_AndMovesOn_WithoutPlayingIt()
    {
        List<Guid> idList = await AddMedia(2);
        _audio.LoadFailure = "The file could not be played.";

        await Start(idList[0]);
        // Both items were tried (the load of each fails) and the last one is the one left, failed.
        await WaitUntil(() => LoadList.Count == 2 && _player.Current is { } current && _player.StateOf(current) == FileSyncStateEnum.Failed && Snapshot().HistoryList.Count == 1);

        Assert.DoesNotContain("Play", _audio.CallList);
        Assert.False(_player.IsPlaying);
    }

    #endregion -- Failures --

    #region -- Prefetch --

    [Fact]
    public async Task Prefetch_RequestsTheNextThreeMissingMediaFiles_NotMore()
    {
        List<Guid> idList = [await AddMedia(), .. await AddMedia(7, false)];

        await Start(idList[0]);

        List<Guid> requested = [.. _download.EnqueueCallList.SelectMany(x => x)];
        Assert.Equal(idList.Skip(1).Take(3), requested);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public async Task Prefetch_FollowsThePrefetchCountSetting(int count)
    {
        _serviceSetting.PrefetchCount = count;
        List<Guid> idList = [await AddMedia(), .. await AddMedia(8, false)];

        await Start(idList[0]);

        Assert.Equal(idList.Skip(1).Take(count), _download.EnqueueCallList.SelectMany(x => x));
    }

    [Fact]
    public async Task Prefetch_WithTheCountAtZero_AsksForNothing()
    {
        _serviceSetting.PrefetchCount = 0;
        List<Guid> idList = [await AddMedia(), .. await AddMedia(4, false)];

        await Start(idList[0]);

        Assert.Empty(_download.EnqueueCallList);
    }

    [Fact]
    public async Task Prefetch_NeverStartsDownloadsForItemsBeyondTheWindow_AndTheWindowMovesWithTheQueue()
    {
        List<Guid> idList = [await AddMedia(), .. await AddMedia(7, false)];
        DownloadsSucceed();
        await Start(idList[0]);

        await _player.Next();

        List<Guid> requested = [.. _download.EnqueueCallList.SelectMany(x => x)];
        Assert.DoesNotContain(idList[5], requested);
        Assert.Contains(idList[4], requested);
        Assert.DoesNotContain(idList[6], requested);
    }

    [Fact]
    public async Task Prefetch_LeavesAlone_TheMediaFilesAlreadyOnTheDevice()
    {
        List<Guid> idList = await AddMedia(5);

        await Start(idList[0]);

        Assert.Empty(_download.EnqueueCallList.SelectMany(x => x));
    }

    [Fact]
    public async Task Prefetch_LooksAtTheUserQueueFirst()
    {
        List<Guid> idList = [await AddMedia(), .. await AddMedia(5, false)];
        Guid queued = await AddMedia(false);
        await Start(idList[0]);
        await PlayNext(queued);
        _download.EnqueueCallList.Clear();

        await _player.PlayCurrent();

        Assert.Equal([queued, idList[1], idList[2]], _download.EnqueueCallList.SelectMany(x => x));
    }

    #endregion -- Prefetch --

    #region -- Library changes --

    [Fact]
    public async Task ADeletedMediaFile_InTheQueue_IsSkipped_AfterTheCurrentOneFinishes()
    {
        List<Guid> idList = await AddMedia(3);
        await Start(idList[0]);

        _sync.RaiseLibraryDiff(new([], [], [idList[1]]));
        Assert.True(_audio.IsPlaying);
        Assert.Equal(idList[0], _player.Current?.MediaFileID);

        _audio.RaiseEnded();
        await WaitUntil(() => IsPlayingMedia(idList[2]));

        Assert.DoesNotContain(idList[1], MediaOf(Snapshot().SourceList));
        Assert.DoesNotContain(LoadList, x => x.Contains(Json(idList[1])));
    }

    [Fact]
    public async Task ADeletedPlayingMediaFile_FinishesPlaying_ThenTheNextOneFollows()
    {
        List<Guid> idList = await AddMedia(2);
        await Start(idList[0]);
        int callsBefore = _audio.CallList.Count;

        _sync.RaiseLibraryDiff(new([], [], [idList[0]]));

        Assert.Equal(callsBefore, _audio.CallList.Count);
        Assert.True(_audio.IsPlaying);
        Assert.Equal(idList[0], _player.Current?.MediaFileID);

        _audio.RaiseEnded();
        await WaitUntil(() => IsPlayingMedia(idList[1]));

        Assert.Empty(Snapshot().HistoryList);
    }

    [Fact]
    public async Task AChangedHash_ForTheCurrentMediaFile_FinishesPlaying_ThenIsReDownloadedOnNextPlay()
    {
        Guid id = await AddMedia();
        await Start(id);
        ResponseMediaFile changed = (await _storeMetadata.GetMediaFile(id)) ?? throw new InvalidOperationException("Missing.");

        // What the reconciler does for a changed hash: the old bytes go.
        await _storeFile.Delete(id);
        _sync.RaiseLibraryDiff(new([], [changed], []));

        Assert.True(_audio.IsPlaying);
        Assert.Empty(_download.EnsureList);

        DownloadsSucceed();
        await Start(id);

        Assert.Equal([id], _download.EnsureList);
        Assert.True(_audio.IsPlaying);
    }

    [Fact]
    public async Task TheItemState_FollowsTheFileState()
    {
        Guid id = await AddMedia();
        await Start(id);
        QueueItem item = _player.Current ?? throw new InvalidOperationException("No current.");
        Assert.Equal(FileSyncStateEnum.NotDownloaded, _player.StateOf(item));

        await _storeMetadata.SaveFileState(new(id, FileSyncStateEnum.Downloading, null, null));
        _download.RaiseChanged();
        await WaitUntil(() => _player.StateOf(item) == FileSyncStateEnum.Downloading);

        await _storeMetadata.SaveFileState(new(id, FileSyncStateEnum.OnDevice, new('a', 64), null));
        _download.RaiseChanged();
        await WaitUntil(() => _player.StateOf(item) == FileSyncStateEnum.OnDevice);
    }

    #endregion -- Library changes --

    #region -- Controls --

    [Fact]
    public async Task ANewPlayer_IsDisabled()
    {
        Assert.True(_player.IsDisabled);
        Assert.Null(_player.Current);
        Assert.False(_player.PositionJumped);
    }

    [Fact]
    public async Task Clear_DisablesThePlayer()
    {
        Guid id = await AddMedia();
        await Start(id);
        Assert.False(_player.IsDisabled);

        await _player.Clear();

        Assert.True(_player.IsDisabled);
    }

    [Fact]
    public async Task AfterAnAccountSwitchClear_IsDisabled_AndTheLocalCopyIsGone()
    {
        Guid id = await AddMedia();
        await Start(id);
        await _queue.SaveLocalCopy(0);
        Assert.NotNull(await _storeLocalQueue.Get());

        await _player.Clear();

        Assert.True(_player.IsDisabled);
        Assert.Null(await _storeLocalQueue.Get());
        Assert.Null(_player.PlaylistID);
    }

    [Fact]
    public async Task Pause_ThenResume_ControlsTheAudioPlayer()
    {
        Guid id = await AddMedia();
        await Start(id);

        await _player.Pause();
        Assert.False(_audio.IsPlaying);

        await _player.Resume();
        Assert.True(_audio.IsPlaying);
    }

    [Fact]
    public async Task Seek_PassesThePositionOnWhenSomethingIsLoaded()
    {
        Guid id = await AddMedia();
        await Start(id);

        await _player.Seek(TimeSpan.FromSeconds(42));

        Assert.Contains($"Seek {TimeSpan.FromSeconds(42)}", _audio.CallList);
        Assert.Equal(TimeSpan.FromSeconds(42), _player.Position);
    }

    [Fact]
    public async Task ASeek_SetsPositionJumped_AndTheReporterCanClearIt()
    {
        Guid id = await AddMedia();
        await Start(id);
        Assert.False(_player.PositionJumped);

        await _player.Seek(TimeSpan.FromSeconds(5));
        Assert.True(_player.PositionJumped);

        _player.PositionJumped = false;
        Assert.False(_player.PositionJumped);
    }

    [Theory]
    [InlineData(0.4, 0.4)]
    [InlineData(1.7, 1)]
    [InlineData(-3, 0)]
    public async Task SetVolume_IsClampedToZeroAndOne(double asked, double expected)
    {
        await _player.SetVolume(asked);

        Assert.Equal(expected, _player.Volume);
        Assert.Equal(expected, _audio.Volume);
    }

    [Fact]
    public async Task TheVolume_AppliesToTheNextFileToo()
    {
        Guid id = await AddMedia();
        await _player.SetVolume(0.3);

        await Start(id);

        Assert.Equal(0.3, _audio.Volume);
    }

    [Fact]
    public async Task AnEmptyQueue_ResumePauseSeekNextPrevious_DoNothing_AndDoNotThrow()
    {
        await _player.Resume();
        await _player.Pause();
        await _player.Seek(TimeSpan.FromSeconds(5));
        await _player.Next();
        await _player.Previous();

        Assert.DoesNotContain(_audio.CallList, x => x.StartsWith("Load ") || x == "Play" || x.StartsWith("Seek"));
        Assert.Null(_player.Current);
        Assert.Equal(0, _player.Queue.ItemCount);
    }

    [Fact]
    public async Task PositionAndDuration_ComeFromTheAudioPlayer()
    {
        Guid id = await AddMedia();
        await Start(id);

        _audio.Position = TimeSpan.FromSeconds(12);
        _audio.Duration = TimeSpan.FromSeconds(200);

        Assert.Equal(TimeSpan.FromSeconds(12), _player.Position);
        Assert.Equal(TimeSpan.FromSeconds(200), _player.Duration);
    }

    [Fact]
    public async Task APlayerThatNeverPlayed_StillRaisesChanged_WhenTheQueueChanges()
    {
        int changed = 0;
        _player.Changed += () => Interlocked.Increment(ref changed);

        await _queue.ReplaceWithSnapshot(new() { QueueRevision = 1, Source = null, HistoryList = [], Current = new(Guid.NewGuid(), Guid.NewGuid()), UserQueueList = [], SourceList = [] }, false);

        Assert.True(changed > 0);
    }

    [Fact]
    public async Task Changed_IsRaisedWhenTheQueueOrThePlayStateChanges()
    {
        Guid id = await AddMedia();
        int changed = 0;
        _player.Changed += () => Interlocked.Increment(ref changed);

        await Start(id);
        int afterPlay = changed;
        await _player.Pause();

        Assert.True(afterPlay > 0);
        Assert.True(changed > afterPlay);
    }

    #endregion -- Controls --

    #region -- Volume on the device --

    [Fact]
    public async Task TheStoredVolume_IsAppliedWhenTheFirstMediaFileStarts()
    {
        Guid id = await AddMedia();
        await _storeMetadata.SetVolume(35);

        await Start(id);

        Assert.Equal(0.35, _audio.Volume);
        Assert.Equal(0.35, _player.Volume);
        Assert.Empty(_storeMetadata.VolumeWriteList.Skip(1));
    }

    [Fact]
    public async Task LoadVolume_ReadsTheStoredVolume_BeforeAnythingPlays()
    {
        await _storeMetadata.SetVolume(20);

        await _player.LoadVolume();

        Assert.Equal(0.2, _player.Volume);
        Assert.Empty(_audio.CallList);
    }

    [Fact]
    public async Task TheStoredVolume_IsReadOnlyOnce_SoALaterStartKeepsTheLocalVolume()
    {
        Guid id = await AddMedia();
        await _storeMetadata.SetVolume(35);
        await Start(id);
        await _player.SetVolume(0.8);
        await _storeMetadata.SetVolume(10);

        await Start(id);

        Assert.Equal(0.8, _audio.Volume);
    }

    [Fact]
    public async Task SetVolume_AppliesAtOnce_AndStoresItLocallyRounded()
    {
        int before = _storeMetadata.VolumeWriteList.Count;

        await _player.SetVolume(0.456);

        Assert.Equal(0.456, _audio.Volume);
        Assert.Equal([46], _storeMetadata.VolumeWriteList.Skip(before));
        Assert.Equal(46, await _storeMetadata.GetVolume());
    }

    [Fact]
    public async Task PreviewVolume_AppliesAtOnce_AndStoresNothing()
    {
        int before = _storeMetadata.VolumeWriteList.Count;

        await _player.PreviewVolume(0.456);

        Assert.Equal(0.456, _audio.Volume);
        Assert.Equal(0.456, _player.Volume);
        Assert.Equal(before, _storeMetadata.VolumeWriteList.Count);
    }

    [Theory]
    [InlineData(1.7, 1)]
    [InlineData(-3, 0)]
    public async Task PreviewVolume_IsClampedToZeroAndOne(double asked, double expected)
    {
        await _player.PreviewVolume(asked);

        Assert.Equal(expected, _player.Volume);
        Assert.Equal(expected, _audio.Volume);
    }

    [Fact]
    public async Task PreviewVolume_RaisesChanged_SoTheScreensFollowTheSlider()
    {
        int changed = 0;
        _player.Changed += () => changed++;

        await _player.PreviewVolume(0.5);

        Assert.True(changed > 0);
    }

    [Fact]
    public async Task PreviewVolume_BeforeTheFirstStart_IsNotReplacedByTheStoredVolume()
    {
        Guid id = await AddMedia();
        await _storeMetadata.SetVolume(35);
        await _player.PreviewVolume(0.6);

        await Start(id);

        Assert.Equal(0.6, _audio.Volume);
    }

    [Fact]
    public async Task SetVolume_BeforeTheFirstStart_IsNotReplacedByTheStoredVolume()
    {
        Guid id = await AddMedia();
        await _storeMetadata.SetVolume(35);
        await _player.SetVolume(0.6);

        await Start(id);

        Assert.Equal(0.6, _audio.Volume);
    }

    [Fact]
    public async Task SetVolume_WhenTheLocalSaveFails_KeepsTheVolume_AndLogsIt()
    {
        _storeMetadata.VolumeWriteFailure = new IOException("The disk is full.");

        await _player.SetVolume(0.7);

        Assert.Equal(0.7, _player.Volume);
        Assert.Equal(0.7, _audio.Volume);
        Assert.Contains(_logger.EntryList, x => x.Level == LogLevel.Warning);
        Assert.DoesNotContain(_logger.EntryList, x => x.Message.Contains("0.7") || x.Message.Contains("70"));
    }

    [Fact]
    public async Task TheVolume_SurvivesARestart()
    {
        await _player.SetVolume(0.6);
        PlayerService restarted = new(_audio, _download, _storeFile, _storeMetadata, _sync, _storeMetadata, _queue, new QueueSourceService(_storeMetadata, new ShuffleService(new(1)), new(1)), _serviceSetting, _api, _delay, _logger);

        await restarted.LoadVolume();

        Assert.Equal(0.6, restarted.Volume);
    }

    [Fact]
    public async Task AStoredVolumeThatCannotBeRead_DoesNotBlockPlayback()
    {
        Guid id = await AddMedia();
        _storeMetadata.VolumeReadFailure = new IOException("The store is not readable.");

        await Start(id);

        Assert.Contains("Play", _audio.CallList);
        Assert.Equal(1, _player.Volume);
        Assert.Contains(_logger.EntryList, x => x.Level == LogLevel.Warning);
    }

    #endregion -- Volume on the device --
}
