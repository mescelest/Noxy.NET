using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Presentation;
using Noxy.NET.MediaPlayer.Presentation.Enums;
using Noxy.NET.MediaPlayer.Presentation.Services;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class PlayerServiceTest
{
    private readonly FakeAudioPlayer _audio = new();
    private readonly FakeDownloadService _download = new();
    private readonly FakeFileStore _storeFile = new();
    private readonly FakeMetadataStore _storeMetadata = new();
    private readonly FakeSyncService _sync = new();
    private readonly FakeLogger<PlayerService> _logger = new();
    private readonly PlayerService _player;

    public PlayerServiceTest()
    {
        _player = new(_audio, _download, _storeFile, _storeMetadata, _sync, _logger);
    }

    // A media file in the library; its bytes are on the device unless said otherwise.
    private async Task<Guid> AddMedia(bool onDevice = true)
    {
        ResponseMediaFile media = new()
        {
            ID = Guid.CreateVersion7(),
            Title = "Media",
            ContentType = "audio/mpeg",
            SizeBytes = 3,
            ContentHash = new string('a', 64),
            TimeCreated = DateTime.UtcNow,
            TimeModified = DateTime.UtcNow
        };
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

    private List<string> LoadList => _audio.CallList.Where(x => x.StartsWith("Load ")).ToList();

    #region -- Starting --

    [Fact]
    public async Task Play_WhenOnDevice_LoadsTheStoredPath_AndPlays_WithNoDownload()
    {
        Guid id = await AddMedia();

        await _player.Play(id);

        Assert.Equal([$"Load {await PathOf(id)}"], LoadList);
        Assert.Contains("Play", _audio.CallList);
        Assert.Empty(_download.EnsureList);
        Assert.Equal(id, _player.Current?.MediaFileID);
        Assert.True(_audio.IsPlaying);
    }

    [Fact]
    public async Task Play_WhenMissing_DownloadsFirst_ThenPlays_AndIsWaitingForDownloadInBetween()
    {
        Guid id = await AddMedia(false);
        TaskCompletionSource gate = new(TaskCreationOptions.RunContinuationsAsynchronously);
        _download.EnsureHandler = async downloaded =>
        {
            await gate.Task;
            _storeFile.Hold(downloaded, [1, 2, 3]);

            return true;
        };

        Task play = _player.Play(id);
        await WaitUntil(() => _player.IsWaitingForDownload);

        Assert.Empty(LoadList);
        Assert.False(_audio.IsPlaying);

        gate.SetResult();
        await play;

        Assert.False(_player.IsWaitingForDownload);
        Assert.Equal([$"Load {await PathOf(id)}"], LoadList);
        Assert.True(_audio.IsPlaying);
    }

    [Fact]
    public async Task Play_WhenTheDownloadFails_DoesNotPlay_AndSkipsToTheNextPlayableEntry()
    {
        Guid bad = await AddMedia(false);
        Guid good = await AddMedia();
        _download.EnsureResult = false;

        await _player.PlayList([bad, good], 0);

        Assert.Equal(good, _player.Current?.MediaFileID);
        Assert.Equal([$"Load {await PathOf(good)}"], LoadList);
        Assert.Equal(FileSyncStateEnum.Failed, _player.QueueList[0].State);
    }

    [Fact]
    public async Task Play_WhenEveryEntryFails_PlaysNothing_AndDoesNotThrow()
    {
        List<Guid> idList = await AddMedia(2, false);
        _download.EnsureResult = false;

        await _player.PlayList(idList, 0);

        Assert.Empty(LoadList);
        Assert.False(_player.IsPlaying);
        Assert.All(_player.QueueList, x => Assert.Equal(FileSyncStateEnum.Failed, x.State));
    }

    [Fact]
    public async Task Play_ForAMediaFileThatIsNotInTheLibrary_DoesNotPlay()
    {
        await _player.Play(Guid.CreateVersion7());

        Assert.Empty(LoadList);
        Assert.Empty(_download.EnsureList);
    }

    [Fact]
    public async Task PlayList_StartsAtTheGivenIndex_AndClampsAnIndexOutsideTheList()
    {
        List<Guid> idList = await AddMedia(3);

        await _player.PlayList(idList, 2);
        Assert.Equal(idList[2], _player.Current?.MediaFileID);

        await _player.PlayList(idList, 99);
        Assert.Equal(idList[2], _player.Current?.MediaFileID);

        await _player.PlayList(idList, -4);
        Assert.Equal(idList[0], _player.Current?.MediaFileID);
        Assert.Equal(idList, _player.QueueList.Select(x => x.MediaFileID));
    }

    [Fact]
    public async Task Play_ReplacesTheQueueWithThatMediaFile()
    {
        List<Guid> idList = await AddMedia(3);
        await _player.PlayList(idList, 0);

        await _player.Play(idList[2]);

        Assert.Equal([idList[2]], _player.QueueList.Select(x => x.MediaFileID));
    }

    [Fact]
    public async Task APlayThatIsSupersededWhileItDownloads_NeverLoadsItsFile()
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
        Task first = _player.Play(slow);
        await WaitUntil(() => _player.IsWaitingForDownload);

        await _player.Play(other);
        gate.SetResult();
        await first;

        Assert.Equal([$"Load {await PathOf(other)}"], LoadList);
        Assert.Equal(other, _player.Current?.MediaFileID);
    }

    #endregion -- Starting --

    #region -- Moving through the queue --

    [Fact]
    public async Task Ended_AdvancesToTheNextEntry()
    {
        List<Guid> idList = await AddMedia(2);
        await _player.PlayList(idList, 0);

        _audio.RaiseEnded();
        await WaitUntil(() => _player.Current?.MediaFileID == idList[1]);

        Assert.Equal([$"Load {await PathOf(idList[0])}", $"Load {await PathOf(idList[1])}"], LoadList);
        Assert.True(_audio.IsPlaying);
    }

    [Fact]
    public async Task Ended_AtTheEndOfTheQueue_Stops_AndReleasesTheFile()
    {
        Guid id = await AddMedia();
        await _player.Play(id);

        _audio.RaiseEnded();
        await WaitUntil(() => _audio.LoadedPath is null);

        Assert.False(_audio.IsPlaying);
        Assert.Equal("Stop", _audio.CallList.Last());
    }

    [Fact]
    public async Task Next_MovesToTheNextEntry_AndAtTheEnd_DoesNothing()
    {
        List<Guid> idList = await AddMedia(2);
        await _player.PlayList(idList, 0);

        await _player.Next();
        Assert.Equal(idList[1], _player.Current?.MediaFileID);
        int calls = _audio.CallList.Count;

        await _player.Next();

        Assert.Equal(idList[1], _player.Current?.MediaFileID);
        Assert.Equal(calls, _audio.CallList.Count);
        Assert.True(_audio.IsPlaying);
    }

    [Fact]
    public async Task Previous_GoesBack_AndAtTheStart_RestartsTheCurrentMediaFile()
    {
        List<Guid> idList = await AddMedia(2);
        await _player.PlayList(idList, 1);

        await _player.Previous();
        Assert.Equal(idList[0], _player.Current?.MediaFileID);
        _audio.CallList.Clear();

        await _player.Previous();

        Assert.Equal(idList[0], _player.Current?.MediaFileID);
        Assert.Contains($"Seek {TimeSpan.Zero}", _audio.CallList);
        Assert.DoesNotContain(_audio.CallList, x => x.StartsWith("Load "));
    }

    [Fact]
    public async Task Enqueue_ThenPlay_TheSameMediaFileTwice_PlaysItTwice()
    {
        Guid id = await AddMedia();
        await _player.Play(id);
        await _player.Enqueue(id);

        _audio.RaiseEnded();
        await WaitUntil(() => LoadList.Count == 2);

        Assert.Equal(2, _player.QueueList.Count);
        Assert.Equal(LoadList[0], LoadList[1]);
    }

    [Fact]
    public async Task Enqueue_AddsToTheEnd_WithoutStartingToPlay()
    {
        Guid id = await AddMedia();

        await _player.Enqueue(id);

        Assert.Equal([id], _player.QueueList.Select(x => x.MediaFileID));
        Assert.Empty(LoadList);
        Assert.False(_player.IsPlaying);
    }

    [Fact]
    public async Task Remove_TheCurrentEntry_PlaysTheNextOne()
    {
        List<Guid> idList = await AddMedia(3);
        await _player.PlayList(idList, 1);

        await _player.Remove(1);

        Assert.Equal(idList[2], _player.Current?.MediaFileID);
        Assert.Equal([idList[0], idList[2]], _player.QueueList.Select(x => x.MediaFileID));
        Assert.Equal(await PathOf(idList[2]), _audio.LoadedPath);
    }

    [Fact]
    public async Task Remove_AnEarlierEntry_KeepsTheCurrentOnePlaying()
    {
        List<Guid> idList = await AddMedia(3);
        await _player.PlayList(idList, 2);
        int loads = LoadList.Count;

        await _player.Remove(0);

        Assert.Equal(idList[2], _player.Current?.MediaFileID);
        Assert.Equal(loads, LoadList.Count);
        Assert.Equal([idList[1], idList[2]], _player.QueueList.Select(x => x.MediaFileID));
    }

    [Fact]
    public async Task Remove_AnIndexOutsideTheQueue_IsHarmless()
    {
        Guid id = await AddMedia();
        await _player.Play(id);

        await _player.Remove(5);
        await _player.Remove(-1);

        Assert.Equal([id], _player.QueueList.Select(x => x.MediaFileID));
    }

    [Fact]
    public async Task Remove_TheLastEntryWhilePlaying_Stops()
    {
        Guid id = await AddMedia();
        await _player.Play(id);

        await _player.Remove(0);

        Assert.Empty(_player.QueueList);
        Assert.Null(_player.Current);
        Assert.Equal("Stop", _audio.CallList.Last());
    }

    [Fact]
    public async Task Clear_StopsAndEmptiesTheQueue()
    {
        List<Guid> idList = await AddMedia(2);
        await _player.PlayList(idList, 0);

        await _player.Clear();

        Assert.Empty(_player.QueueList);
        Assert.Null(_player.Current);
        Assert.Equal("Stop", _audio.CallList.Last());
        Assert.False(_player.IsPlaying);
    }

    [Fact]
    public async Task TheAudioPlayer_IsStoppedBeforeEveryLoad_SoThePreviousFileIsNeverHeldOpen()
    {
        List<Guid> idList = await AddMedia(3);
        await _player.PlayList(idList, 0);
        await _player.Next();
        await _player.Next();

        for (int i = 0; i < _audio.CallList.Count; i++)
        {
            if (_audio.CallList[i].StartsWith("Load ")) Assert.Equal("Stop", _audio.CallList[i - 1]);
        }

        Assert.Equal(3, LoadList.Count);
    }

    #endregion -- Moving through the queue --

    #region -- Failures --

    [Fact]
    public async Task Failed_FromTheAudioPlayer_MarksTheEntryFailed_AndMovesOn_WithoutDeletingTheVerifiedFile()
    {
        List<Guid> idList = await AddMedia(2);
        await _player.PlayList(idList, 0);

        _audio.RaiseFailed("The file could not be played.");
        await WaitUntil(() => _player.Current?.MediaFileID == idList[1]);

        Assert.Equal(FileSyncStateEnum.Failed, _player.QueueList[0].State);
        Assert.NotNull(await _storeFile.GetPath(idList[0]));
        Assert.DoesNotContain(_audio.CallList, x => x.StartsWith("Delete"));
    }

    [Fact]
    public async Task Failed_OnTheLastEntry_StopsWithoutThrowing()
    {
        Guid id = await AddMedia();
        await _player.Play(id);

        _audio.RaiseFailed("The file could not be played.");
        await WaitUntil(() => _player.QueueList[0].State == FileSyncStateEnum.Failed);

        Assert.False(_player.IsPlaying);
        Assert.NotNull(await _storeFile.GetPath(id));
    }

    [Fact]
    public async Task AFileTheAudioPlayerCannotLoad_FailsTheEntry_AndMovesOn_WithoutPlayingIt()
    {
        List<Guid> idList = await AddMedia(2);
        _audio.LoadFailure = "The file could not be played.";

        await _player.PlayList(idList, 0);
        await WaitUntil(() => _player.QueueList.All(x => x.State == FileSyncStateEnum.Failed));

        Assert.DoesNotContain("Play", _audio.CallList);
        Assert.False(_player.IsPlaying);
    }

    #endregion -- Failures --

    #region -- Prefetch --

    [Fact]
    public async Task Prefetch_RequestsTheNextThreeMissingMediaFiles_NotMore()
    {
        List<Guid> idList = [await AddMedia(), .. await AddMedia(7, false)];

        await _player.PlayList(idList, 0);

        List<Guid> requested = [.. _download.EnqueueCallList.SelectMany(x => x)];
        Assert.Equal(idList.Skip(1).Take(Constants.PrefetchCount), requested);
    }

    [Fact]
    public async Task Prefetch_NeverStartsDownloadsForEntriesBeyondTheWindow_AndTheWindowMovesWithTheQueue()
    {
        List<Guid> idList = [await AddMedia(), .. await AddMedia(7, false)];
        DownloadsSucceed();
        await _player.PlayList(idList, 0);

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

        await _player.PlayList(idList, 0);

        Assert.Empty(_download.EnqueueCallList.SelectMany(x => x));
    }

    #endregion -- Prefetch --

    #region -- Library changes --

    [Fact]
    public async Task ADeletedMediaFile_InTheQueue_IsSkipped_AfterTheCurrentOneFinishes()
    {
        List<Guid> idList = await AddMedia(3);
        await _player.PlayList(idList, 0);

        _sync.RaiseLibraryDiff(new([], [], [idList[1]]));
        Assert.True(_audio.IsPlaying);
        Assert.Equal(idList[0], _player.Current?.MediaFileID);

        _audio.RaiseEnded();
        await WaitUntil(() => _player.Current?.MediaFileID == idList[2]);

        Assert.DoesNotContain(idList[1], _player.QueueList.Select(x => x.MediaFileID));
        Assert.DoesNotContain(LoadList, x => x.Contains(idList[1].ToString()));
    }

    [Fact]
    public async Task ADeletedCurrentMediaFile_FinishesPlaying_ThenIsSkipped()
    {
        List<Guid> idList = await AddMedia(2);
        await _player.PlayList(idList, 0);
        int callsBefore = _audio.CallList.Count;

        _sync.RaiseLibraryDiff(new([], [], [idList[0]]));

        Assert.Equal(callsBefore, _audio.CallList.Count);
        Assert.True(_audio.IsPlaying);
        Assert.Equal(idList[0], _player.Current?.MediaFileID);

        _audio.RaiseEnded();
        await WaitUntil(() => _player.Current?.MediaFileID == idList[1]);
    }

    [Fact]
    public async Task AChangedHash_ForTheCurrentMediaFile_FinishesPlaying_ThenIsReDownloadedOnNextPlay()
    {
        Guid id = await AddMedia();
        await _player.Play(id);
        ResponseMediaFile changed = (await _storeMetadata.GetMediaFile(id)) ?? throw new InvalidOperationException("Missing.");

        // What the reconciler does for a changed hash: the old bytes go.
        await _storeFile.Delete(id);
        _sync.RaiseLibraryDiff(new([], [changed], []));

        Assert.True(_audio.IsPlaying);
        Assert.Empty(_download.EnsureList);

        DownloadsSucceed();
        await _player.Play(id);

        Assert.Equal([id], _download.EnsureList);
        Assert.True(_audio.IsPlaying);
    }

    [Fact]
    public async Task TheQueueEntryState_FollowsTheFileState()
    {
        Guid id = await AddMedia();
        await _player.Enqueue(id);
        Assert.Equal(FileSyncStateEnum.NotDownloaded, _player.QueueList.Single().State);

        await _storeMetadata.SaveFileState(new(id, FileSyncStateEnum.Downloading, null, null));
        _download.RaiseChanged();
        await WaitUntil(() => _player.QueueList.Single().State == FileSyncStateEnum.Downloading);

        await _storeMetadata.SaveFileState(new(id, FileSyncStateEnum.OnDevice, new string('a', 64), null));
        _download.RaiseChanged();
        await WaitUntil(() => _player.QueueList.Single().State == FileSyncStateEnum.OnDevice);
    }

    #endregion -- Library changes --

    #region -- Controls --

    [Fact]
    public async Task Pause_ThenResume_ControlsTheAudioPlayer()
    {
        Guid id = await AddMedia();
        await _player.Play(id);

        await _player.Pause();
        Assert.False(_audio.IsPlaying);

        await _player.Resume();
        Assert.True(_audio.IsPlaying);
    }

    [Fact]
    public async Task Seek_PassesThePositionOnWhenSomethingIsLoaded()
    {
        Guid id = await AddMedia();
        await _player.Play(id);

        await _player.Seek(TimeSpan.FromSeconds(42));

        Assert.Contains($"Seek {TimeSpan.FromSeconds(42)}", _audio.CallList);
        Assert.Equal(TimeSpan.FromSeconds(42), _player.Position);
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

        await _player.Play(id);

        Assert.Equal(0.3, _audio.Volume);
    }

    [Fact]
    public async Task AnEmptyQueue_ResumePauseSeekNextPreviousAndRemove_DoNothing_AndDoNotThrow()
    {
        await _player.Resume();
        await _player.Pause();
        await _player.Seek(TimeSpan.FromSeconds(5));
        await _player.Next();
        await _player.Previous();
        await _player.Remove(0);

        Assert.DoesNotContain(_audio.CallList, x => x.StartsWith("Load ") || x == "Play" || x.StartsWith("Seek"));
        Assert.Null(_player.Current);
        Assert.Empty(_player.QueueList);
    }

    [Fact]
    public async Task PositionAndDuration_ComeFromTheAudioPlayer()
    {
        Guid id = await AddMedia();
        await _player.Play(id);

        _audio.Position = TimeSpan.FromSeconds(12);
        _audio.Duration = TimeSpan.FromSeconds(200);

        Assert.Equal(TimeSpan.FromSeconds(12), _player.Position);
        Assert.Equal(TimeSpan.FromSeconds(200), _player.Duration);
    }

    [Fact]
    public async Task Changed_IsRaisedWhenTheQueueOrThePlayStateChanges()
    {
        Guid id = await AddMedia();
        int changed = 0;
        _player.Changed += () => Interlocked.Increment(ref changed);

        await _player.Play(id);
        int afterPlay = changed;
        await _player.Pause();

        Assert.True(afterPlay > 0);
        Assert.True(changed > afterPlay);
    }

    #endregion -- Controls --
}
