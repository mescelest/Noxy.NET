using Microsoft.Extensions.Logging;
using Noxy.NET.MediaPlayer.Application.Requests.Remote;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Models;
using Noxy.NET.MediaPlayer.Presentation;
using Noxy.NET.MediaPlayer.Presentation.Enums;
using Noxy.NET.MediaPlayer.Presentation.Services;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class PlaybackControlServiceTest
{
    private readonly FakePlayerService _player = new();
    private readonly FakeQueueService _queue = new();
    private readonly FakeRemotePlaybackService _remote = new();
    private readonly FakeMediaHubService _hub = new() { IsConnected = true };
    private readonly FakePlaybackReporterService _reporter = new();
    private readonly FakeDeviceStore _storeDevice = new();
    private readonly FakeDelay _delay = new() { IsManual = true };
    private readonly FakeLogger<PlaybackControlService> _logger = new();
    private readonly PlaybackControlService _control;

    public PlaybackControlServiceTest()
    {
        _control = Create();
        _queue.Queue = LiveQueue.From(new() { QueueRevision = 1, Source = null, HistoryList = [], Current = new(Guid.NewGuid(), Guid.NewGuid()), UserQueueList = [], SourceList = [] }, 50);
        _hub.InvokeHandler = (method, _) => method == MediaHubMethodEnum.SendCommand ? CommandResultEnum.Sent : null;
    }

    private PlaybackControlService Create()
    {
        return new(_player, _queue, _remote, _hub, _reporter, new DeviceService(_storeDevice), _delay, _logger);
    }

    private void AsController()
    {
        _remote.Role = PlaybackRoleEnum.Controller;
        _remote.ActiveDeviceID = Guid.NewGuid();
        _remote.HasSession = true;
    }

    private List<RequestPlayerCommand> CommandList()
    {
        return _hub.ArgumentList(MediaHubMethodEnum.SendCommand).Cast<RequestPlayerCommand>().ToList();
    }

    private List<QueueEdit> EditList()
    {
        return _hub.ArgumentList(MediaHubMethodEnum.EditQueue).Cast<QueueEdit>().ToList();
    }

    // A call that supersedes another cancels it first, so its own wait starts a moment later: a test waits for it before it lets time pass.
    private async Task WaitForDelayCount(int count)
    {
        DateTime deadline = DateTime.UtcNow.AddSeconds(10);
        while (_delay.DelayList.Count < count)
        {
            Assert.True(DateTime.UtcNow < deadline, "Timed out waiting for the waits to start.");
            await Task.Delay(5, TestContext.Current.CancellationToken);
        }
    }

    // Of a drag, every call but the last ends by being superseded (one may be cancelled before its wait even starts); the last one is the single wait left.
    private async Task WaitForTheLastWait(List<Task> dragList)
    {
        DateTime deadline = DateTime.UtcNow.AddSeconds(10);
        while (!(dragList.Take(dragList.Count - 1).All(x => x.IsCompleted) && _delay.PendingCount == 1))
        {
            Assert.True(DateTime.UtcNow < deadline, "Timed out waiting for the drag to settle.");
            await Task.Delay(5, TestContext.Current.CancellationToken);
        }
    }

    private async Task RunEveryAction()
    {
        Guid id = Guid.NewGuid();
        await _control.Play();
        await _control.Pause();
        await _control.Next();
        await _control.Previous();
        await _control.SetShuffle(ShuffleModeEnum.Random);
        await _control.SetRepeat(RepeatModeEnum.Once);
        await _control.JumpTo(id, true);
        await _control.StartLibrary(id, LibrarySortEnum.Year, true, "abba");
        await _control.StartPlaylist(id, id);
        await _control.PlayNext(id);
        await _control.AddToQueue(id);
        await _control.RemoveFromQueue(id);
        await _control.MoveInQueue(id, id);
    }

    #region -- Routing --

    [Fact]
    public async Task AsAController_EveryCommandAndEdit_IsSentToTheServer_AndTheLocalPlayerAndQueueAreUntouched()
    {
        AsController();

        await RunEveryAction();

        Assert.Equal(
        [
            PlayerCommandEnum.Play, PlayerCommandEnum.Pause, PlayerCommandEnum.Next, PlayerCommandEnum.Previous, PlayerCommandEnum.SetShuffle, PlayerCommandEnum.SetRepeat, PlayerCommandEnum.JumpTo, PlayerCommandEnum.PlayLibrary, PlayerCommandEnum.PlayPlaylist
        ], CommandList().Select(x => x.Command));
        Assert.Equal([QueueEditEnum.PlayNext, QueueEditEnum.AddToQueue, QueueEditEnum.Remove, QueueEditEnum.Move], EditList().Select(x => x.Edit));
        Assert.Empty(_player.CallList);
        Assert.Empty(_queue.ApplyList);
        Assert.Equal(0, _reporter.ClaimNowCount);
    }

    [Fact]
    public async Task AsAController_TheCommandsCarryTheirFields()
    {
        AsController();
        Guid id = Guid.NewGuid();
        Guid entry = Guid.NewGuid();

        await _control.SetShuffle(ShuffleModeEnum.SpreadArtists);
        await _control.SetRepeat(RepeatModeEnum.Forever);
        await _control.JumpTo(id, true);
        await _control.StartLibrary(id, LibrarySortEnum.Year, true, "abba");
        await _control.StartPlaylist(id, entry);

        List<RequestPlayerCommand> list = CommandList();
        Assert.Equal(ShuffleModeEnum.SpreadArtists, list[0].Shuffle);
        Assert.Equal(RepeatModeEnum.Forever, list[1].Repeat);
        Assert.Equal((id, true), (list[2].ItemID, list[2].KeepSkipped));
        Assert.Equal((id, LibrarySortEnum.Year, true, "abba"), (list[3].MediaFileID, list[3].Sort, list[3].Descending, list[3].Search));
        Assert.Equal((id, entry), (list[4].PlaylistID, list[4].EntryID));
    }

    [Fact]
    public async Task AsAController_TheQueueEditsCarryTheirFields()
    {
        AsController();
        Guid id = Guid.NewGuid();
        Guid anchor = Guid.NewGuid();

        await _control.PlayNext(id);
        await _control.MoveInQueue(id, anchor);
        await _control.RemoveFromQueue(id);

        List<QueueEdit> list = EditList();
        Assert.Equal(id, list[0].MediaFileID);
        Assert.NotEqual(Guid.Empty, list[0].ItemID);
        Assert.Equal((id, anchor), (list[1].ItemID, list[1].AfterItemID));
        Assert.Equal(id, list[2].ItemID);
    }

    [Theory]
    [InlineData(PlaybackRoleEnum.Active)]
    [InlineData(PlaybackRoleEnum.Local)]
    public async Task AsActiveOrLocal_EveryCommand_RunsLocally(PlaybackRoleEnum role)
    {
        _remote.Role = role;
        _player.Current = new(Guid.NewGuid(), Guid.NewGuid());
        _player.IsPlaying = true;

        await RunEveryAction();

        Assert.Empty(_hub.CallList);
        Assert.Equal(9, _player.CallList.Count);
        Assert.Equal([QueueEditEnum.PlayNext, QueueEditEnum.AddToQueue, QueueEditEnum.Remove, QueueEditEnum.Move], _queue.ApplyList.Select(x => x.Edit));
    }

    [Fact]
    public async Task AsActiveOrLocal_PlayResumes_AndStartsTheLoadedQueue_WhenNothingIsLoadedYet()
    {
        _player.Current = new(Guid.NewGuid(), Guid.NewGuid());
        _player.IsPlaying = false;
        _player.IsLoaded = false;

        await _control.Play();

        Assert.Equal(["Resume", "PlayCurrent"], _player.CallList);
        Assert.True(_player.IsPlaying);
    }

    [Fact]
    public async Task ANoActiveDeviceAnswer_RunsTheActionLocally_AndThisDeviceClaimsWhenItPlays()
    {
        AsController();
        _hub.InvokeHandler = (_, _) => CommandResultEnum.NoActiveDevice;
        _player.Current = new(Guid.NewGuid(), Guid.NewGuid());

        await _control.Play();

        Assert.Single(CommandList());
        Assert.Equal(["Resume"], _player.CallList);
        Assert.Equal(1, _reporter.ClaimNowCount);
    }

    [Fact]
    public async Task ANoActiveDeviceAnswer_ForAnActionThatDoesNotPlay_DoesNotClaim()
    {
        AsController();
        _hub.InvokeHandler = (_, _) => CommandResultEnum.NoActiveDevice;

        await _control.SetShuffle(ShuffleModeEnum.Random);

        Assert.Equal(["SetShuffle Random"], _player.CallList);
        Assert.Equal(0, _reporter.ClaimNowCount);
    }

    [Fact]
    public async Task AnInvalidAnswer_DoesNothingHere_AndIsLogged()
    {
        AsController();
        _hub.InvokeHandler = (_, _) => CommandResultEnum.Invalid;

        await _control.Next();

        Assert.Empty(_player.CallList);
        Assert.Contains(_logger.EntryList, x => x.Level == LogLevel.Warning);
    }

    [Fact]
    public async Task StartPlaylist_WhileAnotherDeviceIsActive_SendsItThere_AndDoesNotPlayLocally()
    {
        AsController();
        Guid playlistID = Guid.NewGuid();

        await _control.StartPlaylist(playlistID);

        Assert.Equal(PlayerCommandEnum.PlayPlaylist, CommandList().Single().Command);
        Assert.Equal(playlistID, CommandList().Single().PlaylistID);
        Assert.Empty(_player.CallList);
    }

    [Fact]
    public async Task TwoControllers_SendingAtOnce_BothReachTheServer()
    {
        AsController();
        PlaybackControlService other = Create();

        await Task.WhenAll(_control.Next(), other.Previous());

        Assert.Equal([PlayerCommandEnum.Next, PlayerCommandEnum.Previous], CommandList().Select(x => x.Command).Order());
    }

    [Fact]
    public async Task PlayNext_AndAddToQueue_WithNothingLoaded_AreIgnored()
    {
        _queue.Queue = new();

        await _control.PlayNext(Guid.NewGuid());
        await _control.AddToQueue(Guid.NewGuid());
        AsController();
        await _control.PlayNext(Guid.NewGuid());

        Assert.Empty(_queue.ApplyList);
        Assert.Empty(_hub.CallList);
    }

    #endregion -- Routing --

    #region -- Seek and volume --

    [Fact]
    public async Task Seek_ADragOfManySteps_SendsOneCommand_WithTheLastValue_After300ms()
    {
        AsController();
        List<Task> dragList = [];
        for (int step = 1; step <= 10; step++) dragList.Add(_control.Seek(TimeSpan.FromSeconds(step)));

        Assert.Empty(CommandList());
        Assert.All(_delay.DelayList, x => Assert.Equal(TimeSpan.FromMilliseconds(Constants.RemoteDeferMilliseconds), x));

        await WaitForTheLastWait(dragList);
        _delay.ReleaseAll();
        await Task.WhenAll(dragList);

        RequestPlayerCommand command = Assert.Single(CommandList());
        Assert.Equal(PlayerCommandEnum.Seek, command.Command);
        Assert.Equal(10, command.PositionSeconds);
        Assert.Empty(_player.CallList);
    }

    [Fact]
    public async Task SetVolume_ADrag_SendsOneCommand()
    {
        AsController();
        List<Task> dragList = [];
        for (int volume = 10; volume <= 60; volume += 10) dragList.Add(_control.SetVolume(volume));

        await WaitForTheLastWait(dragList);
        _delay.ReleaseAll();
        await Task.WhenAll(dragList);

        RequestPlayerCommand command = Assert.Single(CommandList());
        Assert.Equal(PlayerCommandEnum.SetVolume, command.Command);
        Assert.Equal(60, command.Volume);
        Assert.Empty(_player.CallList);
    }

    [Fact]
    public async Task ASeekAfterThePauseOfADrag_SendsAnother()
    {
        AsController();
        Task first = _control.Seek(TimeSpan.FromSeconds(5));
        await WaitForDelayCount(1);
        _delay.ReleaseAll();
        await first;

        Task second = _control.Seek(TimeSpan.FromSeconds(9));
        await WaitForDelayCount(2);
        _delay.ReleaseAll();
        await second;

        Assert.Equal([5.0, 9.0], CommandList().Select(x => x.PositionSeconds));
    }

    [Fact]
    public async Task ASeekAndAVolumeDrag_DoNotCancelEachOther()
    {
        AsController();
        Task seek = _control.Seek(TimeSpan.FromSeconds(5));
        Task volume = _control.SetVolume(30);

        await WaitForDelayCount(2);
        _delay.ReleaseAll();
        await Task.WhenAll(seek, volume);

        Assert.Equal([PlayerCommandEnum.Seek, PlayerCommandEnum.SetVolume], CommandList().Select(x => x.Command).Order());
    }

    [Fact]
    public async Task NotAsAController_SeekAndVolume_ChangeTheLocalPlayerAtOnce_WithNoDelay()
    {
        _remote.Role = PlaybackRoleEnum.Active;

        await _control.Seek(TimeSpan.FromSeconds(7));
        await _control.SetVolume(150);

        Assert.Equal([$"Seek {TimeSpan.FromSeconds(7)}", $"SetVolume {1.0}"], _player.CallList);
        Assert.Empty(_delay.DelayList);
        Assert.Empty(_hub.CallList);
    }

    [Fact]
    public async Task ADeferredSeek_WhenNobodyIsActive_RunsLocally()
    {
        AsController();
        _hub.InvokeHandler = (_, _) => CommandResultEnum.NoActiveDevice;
        Task seek = _control.Seek(TimeSpan.FromSeconds(12));

        await WaitForDelayCount(1);
        _delay.ReleaseAll();
        await seek;

        Assert.Equal([$"Seek {TimeSpan.FromSeconds(12)}"], _player.CallList);
    }

    #endregion -- Seek and volume --

    #region -- Transfer --

    [Fact]
    public async Task Transfer_WhileOffline_Throws_WithTheOfflineReason()
    {
        _remote.IsOnline = false;
        _remote.HasSession = true;

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _control.Transfer(Guid.NewGuid()));

        Assert.Contains("offline", exception.Message);
        Assert.Empty(_hub.CallList);
    }

    [Fact]
    public async Task Transfer_WithoutALiveSession_Throws()
    {
        _remote.HasSession = false;

        await Assert.ThrowsAsync<InvalidOperationException>(() => _control.Transfer(Guid.NewGuid()));

        Assert.Empty(_hub.CallList);
    }

    [Fact]
    public async Task Transfer_ToThisDevice_IsANoOp_ExceptForAControllerThatAsksForItself()
    {
        _remote.HasSession = true;
        Guid me = await new DeviceService(_storeDevice).GetDeviceID();
        _hub.InvokeHandler = (_, _) => true;

        _remote.Role = PlaybackRoleEnum.Active;
        await _control.Transfer(me);
        Assert.Empty(_hub.CallList);

        AsController();
        await _control.Transfer(me);

        Assert.Equal([me], _hub.ArgumentList(MediaHubMethodEnum.Transfer));
    }

    [Fact]
    public async Task Transfer_Sends_TheTransferMethod_WithTheTargetID()
    {
        _remote.HasSession = true;
        Guid target = Guid.NewGuid();
        _hub.InvokeHandler = (_, _) => true;

        await _control.Transfer(target);

        Assert.Equal([target], _hub.ArgumentList(MediaHubMethodEnum.Transfer));
    }

    [Fact]
    public async Task Transfer_TheServerDoesNotStart_Throws()
    {
        _remote.HasSession = true;
        _hub.InvokeHandler = (_, _) => false;

        await Assert.ThrowsAsync<InvalidOperationException>(() => _control.Transfer(Guid.NewGuid()));
    }

    [Fact]
    public void ATransferFailedPush_IsExposed()
    {
        int failed = 0;
        _control.TransferFailed += () => failed++;

        _remote.RaiseTransferFailed();

        Assert.Equal(1, failed);
    }

    #endregion -- Transfer --

    #region -- Continue --

    [Fact]
    public void CanContinue_WhenALocalCopyExistsAndNoDeviceIsActive()
    {
        _remote.Role = PlaybackRoleEnum.Local;

        Assert.True(_control.CanContinue);
    }

    [Fact]
    public void CanContinue_IsStillTrue_WhenASessionIsLiveButItsActiveDeviceDropped()
    {
        _remote.Role = PlaybackRoleEnum.Local;
        _remote.HasSession = true;
        _remote.ActiveDeviceID = null;

        Assert.True(_control.CanContinue);
    }

    [Fact]
    public void CanContinue_IsFalse_ForAController()
    {
        _remote.Role = PlaybackRoleEnum.Controller;

        Assert.False(_control.CanContinue);
    }

    [Fact]
    public void CanContinue_IsTrue_ForADeviceThatIsStillTheActiveOneAfterARestart_WithNothingPlaying()
    {
        _remote.Role = PlaybackRoleEnum.Active;
        _player.IsPlaying = false;

        Assert.True(_control.CanContinue);
    }

    [Fact]
    public void CanContinue_IsFalse_WhileSomethingPlays_OrThereIsNoQueue()
    {
        _player.IsPlaying = true;
        Assert.False(_control.CanContinue);

        _player.IsPlaying = false;
        _queue.Queue = new();
        Assert.False(_control.CanContinue);
    }

    [Fact]
    public async Task Continue_LoadsTheLocalCopy_AtTheSavedPosition()
    {
        _queue.LocalCopyPosition = TimeSpan.FromSeconds(73);
        _player.Current = new(Guid.NewGuid(), Guid.NewGuid());

        bool continued = await _control.Continue();

        Assert.True(continued);
        Assert.Equal(1, _queue.LoadLocalCopyCount);
        Assert.Equal(["PlayCurrent", $"Seek {TimeSpan.FromSeconds(73)}"], _player.CallList);
    }

    [Fact]
    public async Task Continue_FromTheStartOfTheItem_DoesNotSeek()
    {
        _queue.LocalCopyPosition = TimeSpan.Zero;
        _player.Current = new(Guid.NewGuid(), Guid.NewGuid());

        await _control.Continue();

        Assert.Equal(["PlayCurrent"], _player.CallList);
    }

    [Fact]
    public async Task Continue_WithNoCopy_ReturnsFalse()
    {
        _queue.LocalCopyPosition = null;

        Assert.False(await _control.Continue());

        Assert.Empty(_player.CallList);
    }

    [Fact]
    public async Task Continue_WhenNothingCanBeContinued_DoesNothing()
    {
        _remote.Role = PlaybackRoleEnum.Controller;

        Assert.False(await _control.Continue());

        Assert.Equal(0, _queue.LoadLocalCopyCount);
    }

    #endregion -- Continue --
}
