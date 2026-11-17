using Microsoft.Extensions.Logging;
using Noxy.NET.MediaPlayer.Application.Requests.Remote;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Models;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Presentation;
using Noxy.NET.MediaPlayer.Presentation.Enums;
using Noxy.NET.MediaPlayer.Presentation.Services;
using Noxy.NET.Test.MediaPlayer.Fakes;
using Noxy.NET.Test.MediaPlayer.Fixtures;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class PlaybackReporterServiceTest
{
    private readonly FakePlayerService _player = new();
    private readonly FakeQueueService _queue = new();
    private readonly FakeRemotePlaybackService _remote = new();
    private readonly FakeMediaHubService _hub = new() { IsConnected = true };
    private readonly FakeMetadataStore _storeMetadata = new();
    private readonly FakeDelay _delay = new() { IsManual = true };
    private readonly FakeLogger<PlaybackReporterService> _logger = new();
    private readonly PlaybackReporterService _reporter;

    public PlaybackReporterServiceTest()
    {
        _reporter = new(_player, _queue, _remote, _hub, _storeMetadata, _delay, _logger);
        _reporter.Start();
    }

    private async Task<QueueItem> Playing(string title = "Title", string? artist = "Artist")
    {
        ResponseMediaFile media = MediaFileFactory.Create(title, artist);
        await _storeMetadata.SaveMediaFile(media);
        QueueItem item = new(Guid.NewGuid(), media.ID);
        _player.Current = item;
        _player.IsPlaying = true;

        return item;
    }

    private List<RequestReportPlayerState> ReportList()
    {
        return _hub.ArgumentList(MediaHubMethodEnum.ReportState).Cast<RequestReportPlayerState>().ToList();
    }

    private List<RequestClaimActive> ClaimList()
    {
        return _hub.ArgumentList(MediaHubMethodEnum.ClaimActive).Cast<RequestClaimActive>().ToList();
    }

    private List<QueueEdit> EditList()
    {
        return _hub.ArgumentList(MediaHubMethodEnum.EditQueue).Cast<QueueEdit>().ToList();
    }

    private void Active()
    {
        _remote.Role = PlaybackRoleEnum.Active;
        _remote.ActiveDeviceID = Guid.NewGuid();
        _remote.HasSession = true;
    }

    #region -- Active: reports --

    [Fact]
    public async Task Active_AChangeOfItem_Reports_WithAnIncreasingRevision()
    {
        Active();
        await Playing();
        _player.RaiseChanged();
        QueueItem second = await Playing("Second");
        _player.RaiseChanged();
        await Playing("Third");
        _player.RaiseChanged();

        List<RequestReportPlayerState> reportList = ReportList();
        Assert.Equal(3, reportList.Count);
        Assert.Equal(reportList.Select(x => x.StateRevision).Order(), reportList.Select(x => x.StateRevision));
        Assert.Equal(3, reportList.Select(x => x.StateRevision).Distinct().Count());
        Assert.Equal(second.ItemID, reportList[1].CurrentItemID);
        Assert.Equal("Second", reportList[1].Title);
        Assert.Equal("Artist", reportList[1].Artist);
    }

    [Fact]
    public async Task Active_AReportCarriesThePlayersState()
    {
        Active();
        QueueItem item = await Playing();
        _player.Position = TimeSpan.FromSeconds(12.5);
        _player.Volume = 0.35;
        _player.Shuffle = ShuffleModeEnum.Random;
        _player.Repeat = RepeatModeEnum.Forever;

        _player.RaiseChanged();

        RequestReportPlayerState report = Assert.Single(ReportList());
        Assert.True(report.IsPlaying);
        Assert.Equal(12.5, report.PositionSeconds);
        Assert.Equal(35, report.Volume);
        Assert.Equal(ShuffleModeEnum.Random, report.Shuffle);
        Assert.Equal(RepeatModeEnum.Forever, report.Repeat);
        Assert.Equal(item.ItemID, report.CurrentItemID);
    }

    [Theory]
    [InlineData("pause")]
    [InlineData("seek")]
    [InlineData("volume")]
    [InlineData("shuffle")]
    [InlineData("repeat")]
    public async Task Active_PlayPauseSeekVolumeShuffleRepeat_EachReport(string change)
    {
        Active();
        await Playing();
        _player.RaiseChanged();
        int before = ReportList().Count;

        switch (change)
        {
            case "pause":
                _player.IsPlaying = false;
                break;
            case "seek":
                _player.PositionJumped = true;
                break;
            case "volume":
                _player.Volume = 0.2;
                break;
            case "shuffle":
                _player.Shuffle = ShuffleModeEnum.SpreadArtists;
                break;
            case "repeat":
                _player.Repeat = RepeatModeEnum.Once;
                break;
            default:
                throw new InvalidOperationException(change);
        }

        _player.RaiseChanged();

        Assert.Equal(before + 1, ReportList().Count);
        Assert.False(_player.PositionJumped);
    }

    [Fact]
    public async Task APositionOnlyChange_DoesNotReport()
    {
        Active();
        await Playing();
        _player.RaiseChanged();
        int before = ReportList().Count;

        for (int tick = 1; tick <= 5; tick++)
        {
            _player.Position = TimeSpan.FromSeconds(tick);
            _player.RaiseChanged();
        }

        Assert.Equal(before, ReportList().Count);
    }

    [Fact]
    public async Task Active_AdvanceAndRegenerate_AreSentAsQueueEdits()
    {
        Active();
        await Playing();
        QueueEdit advance = new() { Edit = QueueEditEnum.Advance, Item = new(Guid.NewGuid(), Guid.NewGuid()) };
        QueueEdit regenerate = new() { Edit = QueueEditEnum.RegenerateSource, ItemList = [] };

        _queue.RaiseEditApplied(advance);
        _queue.RaiseEditApplied(regenerate);

        Assert.Equal([advance, regenerate], EditList());
    }

    [Fact]
    public async Task AControllerNeverReports_AndNeverSendsEdits()
    {
        _remote.Role = PlaybackRoleEnum.Controller;
        _remote.ActiveDeviceID = Guid.NewGuid();
        await Playing();

        _player.RaiseChanged();
        _queue.RaiseEditApplied(new() { Edit = QueueEditEnum.Advance });
        await _reporter.ReportNow();

        Assert.Empty(_hub.CallList);
    }

    [Fact]
    public async Task ALocalDevice_NeverSendsEdits_AndAPausedOneOnlyRegisters()
    {
        _remote.Role = PlaybackRoleEnum.Local;
        _queue.RaiseEditApplied(new() { Edit = QueueEditEnum.Advance });
        _player.IsPlaying = false;
        _player.Current = new(Guid.NewGuid(), Guid.NewGuid());

        _player.RaiseChanged();
        _remote.RaiseRegistered();

        Assert.Empty(_hub.CallList);
        await Task.CompletedTask;
    }

    [Fact]
    public async Task ReportNow_SendsEvenWhenNothingChanged_ForTheActiveDevice()
    {
        Active();
        await Playing();
        _player.RaiseChanged();

        await _reporter.ReportNow();

        Assert.Equal(2, ReportList().Count);
    }

    #endregion -- Active: reports --

    #region -- Claiming --

    [Fact]
    public async Task Local_ConnectedAndPlaying_ClaimsActive_WithTheLocalQueue_AndOnAcceptanceReportsFromRevisionOne()
    {
        _hub.InvokeHandler = (method, _) => method == MediaHubMethodEnum.ClaimActive ? true : null;
        QueueItem item = await Playing();
        _queue.Queue = LiveQueue.From(new() { QueueRevision = 7, Source = null, HistoryList = [], Current = item, UserQueueList = [], SourceList = [new(Guid.NewGuid(), Guid.NewGuid())] }, 50);

        _remote.RaiseRegistered();

        RequestClaimActive claim = Assert.Single(ClaimList());
        Assert.Equal(1, claim.State.StateRevision);
        Assert.Equal(item.ItemID, claim.State.CurrentItemID);
        Assert.Equal(7, claim.Queue.QueueRevision);
        Assert.Equal(item, claim.Queue.Current);
        Assert.Single(claim.Queue.SourceList);
        Assert.Equal(1, _remote.MarkActiveCount);
        Assert.Equal(PlaybackRoleEnum.Active, _remote.Role);

        _player.Volume = 0.5;
        _player.RaiseChanged();

        RequestReportPlayerState report = Assert.Single(ReportList());
        Assert.Equal(2, report.StateRevision);
    }

    [Fact]
    public async Task AControllerThatWasPlayingWhenItReconnected_ClaimsToo_AndIsPausedWhenRefused()
    {
        _hub.InvokeHandler = (_, _) => false;
        _remote.Role = PlaybackRoleEnum.Controller;
        _remote.ActiveDeviceID = Guid.NewGuid();
        await Playing();

        _remote.RaiseRegistered();

        Assert.Single(ClaimList());
        Assert.Contains("Pause", _player.CallList);
        Assert.False(_player.IsPlaying);
    }

    [Fact]
    public async Task AControllerThatStartsPlayingOnItsOwn_DoesNotClaim()
    {
        _remote.Role = PlaybackRoleEnum.Controller;
        _remote.ActiveDeviceID = Guid.NewGuid();
        await Playing();

        _player.RaiseChanged();

        Assert.Empty(_hub.CallList);
    }

    [Fact]
    public async Task TheActiveDeviceThatCameBackWithinTheGracePeriod_ClaimsAgainWithItsQueue_SoWhatItPlayedWhileAwayIsNotLost()
    {
        _hub.InvokeHandler = (_, _) => true;
        Active();
        QueueItem moved = await Playing();
        _queue.Queue = LiveQueue.From(new() { QueueRevision = 9, Source = null, HistoryList = [new(Guid.NewGuid(), Guid.NewGuid())], Current = moved, UserQueueList = [], SourceList = [] }, 50);

        _remote.RaiseRegistered();

        RequestClaimActive claim = Assert.Single(ClaimList());
        Assert.Equal(moved, claim.Queue.Current);
        Assert.Single(claim.Queue.HistoryList);
        Assert.Equal(1, claim.State.StateRevision);
    }

    [Fact]
    public async Task Local_ConnectedAndPaused_RegistersOnly_NoClaim()
    {
        await Playing();
        _player.IsPlaying = false;

        _remote.RaiseRegistered();

        Assert.Empty(ClaimList());
    }

    [Fact]
    public async Task Local_StartingToPlayWhileConnected_ClaimsActive()
    {
        _hub.InvokeHandler = (_, _) => true;
        _remote.IsOnline = true;
        await Playing();

        _player.RaiseChanged();

        Assert.Single(ClaimList());
        Assert.Equal(PlaybackRoleEnum.Active, _remote.Role);
    }

    [Fact]
    public async Task ARefusedClaim_PausesThePlayerImmediately()
    {
        _hub.InvokeHandler = (_, _) => false;
        await Playing();

        _remote.RaiseRegistered();

        Assert.Contains("Pause", _player.CallList);
        Assert.False(_player.IsPlaying);
        Assert.Equal(0, _remote.MarkActiveCount);
        Assert.Empty(ReportList());
    }

    [Fact]
    public async Task AClaimTheServerCouldNotAnswer_IsNeitherAcceptedNorRefused()
    {
        _hub.InvokeHandler = (_, _) => null;
        await Playing();

        _remote.RaiseRegistered();

        Assert.True(_player.IsPlaying);
        Assert.Equal(0, _remote.MarkActiveCount);
        Assert.DoesNotContain("Pause", _player.CallList);
    }

    [Fact]
    public async Task Offline_SendsNothing_AndPlaybackContinues()
    {
        _remote.IsOnline = false;
        _hub.IsConnected = false;
        await Playing();

        _player.RaiseChanged();
        _remote.RaiseRegistered();
        _queue.RaiseEditApplied(new() { Edit = QueueEditEnum.Advance });

        Assert.Empty(_hub.CallList);
        Assert.True(_player.IsPlaying);
    }

    [Fact]
    public async Task OnReconnecting_WhilePlaying_ClaimsOnce_WithTheQueueAdvancedWhileOffline()
    {
        _hub.InvokeHandler = (_, _) => true;
        _remote.IsOnline = false;
        QueueItem first = await Playing();
        _player.RaiseChanged();
        QueueItem second = new(Guid.NewGuid(), Guid.NewGuid());
        _player.Current = second;
        _queue.Queue = LiveQueue.From(new() { QueueRevision = 5, Source = null, HistoryList = [first], Current = second, UserQueueList = [], SourceList = [] }, 50);
        _player.RaiseChanged();
        Assert.Empty(_hub.CallList);

        _remote.IsOnline = true;
        _remote.RaiseRegistered();

        RequestClaimActive claim = Assert.Single(ClaimList());
        Assert.Equal(second, claim.Queue.Current);
        Assert.Equal([first], claim.Queue.HistoryList);
        Assert.Equal(second.ItemID, claim.State.CurrentItemID);
    }

    [Fact]
    public async Task EditsMadeWhileAClaimIsOnItsWay_FollowItWhenAccepted_AndAreDroppedWhenRefused()
    {
        TaskCompletionSource gate = new(TaskCreationOptions.RunContinuationsAsynchronously);
        _hub.InvokeHandler = (method, _) => method == MediaHubMethodEnum.ClaimActive ? true : null;
        _hub.Gate = gate.Task;
        await Playing();
        _remote.RaiseRegistered();
        QueueEdit during = new() { Edit = QueueEditEnum.Advance, Item = new(Guid.NewGuid(), Guid.NewGuid()) };

        _queue.RaiseEditApplied(during);
        gate.SetResult();
        await _reporter.ReportNow();

        Assert.Equal([during], EditList());
    }

    #endregion -- Claiming --

    #region -- Sending in order --

    [Fact]
    public async Task ABurstOfChanges_DuringAReport_RunsOneMoreAfterIt_NeverTwoAtOnce()
    {
        TaskCompletionSource gate = new(TaskCreationOptions.RunContinuationsAsynchronously);
        Active();
        await Playing();
        _hub.Gate = gate.Task;
        _player.RaiseChanged();

        for (int change = 0; change < 5; change++)
        {
            _player.Volume = 0.1 + change / 10.0;
            _player.RaiseChanged();
        }

        Assert.Equal(1, _hub.InFlightCount);

        gate.SetResult();
        await _reporter.ReportNow();

        // The one in flight, the one that waited (it read the state as it was by then), and the explicit one.
        Assert.Equal(3, ReportList().Count);
        Assert.Equal(1, _hub.MaxInFlightCount);
        Assert.Equal(50, ReportList()[1].Volume);
    }

    [Fact]
    public async Task AFailedInvoke_IsLoggedByTypeOnly_AndDropped_AndLaterReportsStillGo()
    {
        Active();
        await Playing();
        _hub.InvokeHandler = (_, _) => throw new InvalidOperationException("secret-token queue-content");
        _player.RaiseChanged();
        _hub.InvokeHandler = (_, _) => null;
        _player.Volume = 0.3;
        _player.RaiseChanged();

        Assert.Equal(2, ReportList().Count);
        Assert.Contains(_logger.EntryList, x => x.Level == LogLevel.Warning && x.Message.Contains(nameof(InvalidOperationException)));
        Assert.DoesNotContain(_logger.EntryList, x => x.Message.Contains("secret") || x.Message.Contains("queue-content"));
    }

    [Fact]
    public async Task NoLogLineHoldsATokenOrAQueue()
    {
        _hub.InvokeHandler = (_, _) => throw new HttpRequestException("Bearer abc.def.ghi {\"HistoryList\":[]}");
        await Playing();

        _remote.RaiseRegistered();

        Assert.NotEmpty(_logger.EntryList);
        Assert.All(_logger.EntryList, x =>
        {
            (_, string message) = x;
            Assert.DoesNotContain("Bearer", message);
            Assert.DoesNotContain("HistoryList", message);
        });
    }

    [Fact]
    public async Task Start_CalledTwice_SubscribesOnce()
    {
        Active();
        _reporter.Start();
        await Playing();

        _player.RaiseChanged();

        Assert.Single(ReportList());
    }

    #endregion -- Sending in order --

    #region -- The saved position --

    [Fact]
    public async Task WhilePlaying_TheLocalCopyPositionIsSavedEveryTenSeconds_AndOnPause()
    {
        await Playing();
        _remote.IsOnline = false;
        _player.Position = TimeSpan.FromSeconds(4);
        _player.RaiseChanged();

        Assert.Equal([TimeSpan.FromSeconds(Constants.LocalQueueSaveSeconds)], _delay.DelayList);
        Assert.Empty(_queue.SavedPositionList);

        _player.Position = TimeSpan.FromSeconds(10);
        _delay.ReleaseAll();
        await WaitUntil(() => _queue.SavedPositionList.Count == 1);
        Assert.Equal([10.0], _queue.SavedPositionList);

        _player.Position = TimeSpan.FromSeconds(17);
        _player.IsPlaying = false;
        _player.RaiseChanged();
        await WaitUntil(() => _queue.SavedPositionList.Count == 2);

        Assert.Equal([10.0, 17.0], _queue.SavedPositionList);
        _delay.ReleaseAll();
        await Task.Delay(50, TestContext.Current.CancellationToken);
        Assert.Equal(2, _queue.SavedPositionList.Count);
    }

    [Fact]
    public async Task OnlyOneSaveLoopRuns_HoweverOftenThePlayerChanges()
    {
        await Playing();
        _remote.IsOnline = false;

        for (int tick = 0; tick < 5; tick++) _player.RaiseChanged();

        Assert.Single(_delay.DelayList);
    }

    #endregion -- The saved position --

    [Fact]
    public async Task TheQueueOwnsPlayback_ExceptOnAController()
    {
        Assert.True(_queue.OwnsPlayback);

        _remote.Role = PlaybackRoleEnum.Controller;
        _remote.RaiseChanged();
        Assert.False(_queue.OwnsPlayback);

        _remote.Role = PlaybackRoleEnum.Active;
        _remote.RaiseChanged();
        Assert.True(_queue.OwnsPlayback);

        _remote.Role = PlaybackRoleEnum.Local;
        _remote.RaiseChanged();
        Assert.True(_queue.OwnsPlayback);
        await Task.CompletedTask;
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
}
