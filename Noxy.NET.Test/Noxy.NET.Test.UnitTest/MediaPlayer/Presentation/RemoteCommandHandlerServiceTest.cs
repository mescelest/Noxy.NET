using Microsoft.Extensions.Logging;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Models;
using Noxy.NET.MediaPlayer.Domain.Responses.Remote;
using Noxy.NET.MediaPlayer.Presentation.Enums;
using Noxy.NET.MediaPlayer.Presentation.Models;
using Noxy.NET.MediaPlayer.Presentation.Services;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class RemoteCommandHandlerServiceTest
{
    private readonly FakeMediaHubService _hub = new() { IsConnected = true };
    private readonly FakeRemotePlaybackService _remote = new() { Role = PlaybackRoleEnum.Active };
    private readonly FakePlayerService _player = new();
    private readonly FakeQueueService _queue = new();
    private readonly FakePlaybackReporterService _reporter = new();
    private readonly FakeLogger<RemoteCommandHandlerService> _logger = new();
    private readonly RemoteCommandHandlerService _handler;

    public RemoteCommandHandlerServiceTest()
    {
        _handler = new(_hub, _remote, _player, _queue, _reporter, _logger);
        _handler.Start();
    }

    private static ResponsePlayerState State(long revision = 1, int volume = 50, double position = 0, Guid? activeDeviceID = null)
    {
        return new()
        {
            ActiveDeviceID = activeDeviceID, StateRevision = revision, IsPlaying = false, PositionSeconds = position, ServerTime = DateTime.UtcNow, Volume = volume, Shuffle = ShuffleModeEnum.Off, Repeat = RepeatModeEnum.No, CurrentItemID = null, Title = null,
            Artist = null
        };
    }

    private static ResponseQueue Queue(long revision, QueueItem? current)
    {
        return new() { QueueRevision = revision, Source = null, HistoryList = [], Current = current, UserQueueList = [], SourceList = [] };
    }

    private static QueueItem Item()
    {
        return new(Guid.NewGuid(), Guid.NewGuid());
    }

    #region -- Commands --

    [Fact]
    public void ReceivedPlayAndPause_ControlTheLocalPlayer_AndEachEndsInAReport()
    {
        _player.Current = Item();

        _hub.RaisePlayerCommandReceived(new() { Command = PlayerCommandEnum.Play });
        Assert.True(_player.IsPlaying);
        _hub.RaisePlayerCommandReceived(new() { Command = PlayerCommandEnum.Pause });

        Assert.False(_player.IsPlaying);
        Assert.Equal(["Resume", "Pause"], _player.CallList);
        Assert.Equal(2, _reporter.ReportNowCount);
    }

    [Fact]
    public void ReceivedSeek_SeeksTheLocalPlayer_AndReportsTheNewState()
    {
        _hub.RaisePlayerCommandReceived(new() { Command = PlayerCommandEnum.Seek, PositionSeconds = 42.5 });

        Assert.Equal([$"Seek {TimeSpan.FromSeconds(42.5)}"], _player.CallList);
        Assert.Equal(1, _reporter.ReportNowCount);
    }

    [Fact]
    public void ReceivedNextAndPrevious_MoveTheLocalPlayer()
    {
        _hub.RaisePlayerCommandReceived(new() { Command = PlayerCommandEnum.Next });
        _hub.RaisePlayerCommandReceived(new() { Command = PlayerCommandEnum.Previous });

        Assert.Equal(["Next", "Previous"], _player.CallList);
        Assert.Equal(2, _reporter.ReportNowCount);
    }

    [Fact]
    public void ReceivedSetShuffleAndSetRepeat_SetTheModes()
    {
        _hub.RaisePlayerCommandReceived(new() { Command = PlayerCommandEnum.SetShuffle, Shuffle = ShuffleModeEnum.SpreadArtists });
        _hub.RaisePlayerCommandReceived(new() { Command = PlayerCommandEnum.SetRepeat, Repeat = RepeatModeEnum.Forever });

        Assert.Equal(["SetShuffle SpreadArtists", "SetRepeat Forever"], _player.CallList);
        Assert.Equal(2, _reporter.ReportNowCount);
    }

    [Fact]
    public void ReceivedSetVolume_AppliesItToTheLocalPlayer_AndReportsIt()
    {
        _hub.RaisePlayerCommandReceived(new() { Command = PlayerCommandEnum.SetVolume, Volume = 35 });

        Assert.Equal([$"SetVolume {0.35}"], _player.CallList);
        Assert.Equal(0.35, _player.Volume);
        Assert.Equal(1, _reporter.ReportNowCount);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ReceivedJumpTo_PassesTheItemAndTheFlagToThePlayer_AndReports(bool keepSkipped)
    {
        Guid itemID = Guid.NewGuid();

        _hub.RaisePlayerCommandReceived(new() { Command = PlayerCommandEnum.JumpTo, ItemID = itemID, KeepSkipped = keepSkipped });

        Assert.Equal([$"JumpTo {itemID} {keepSkipped}"], _player.CallList);
        Assert.Equal(1, _reporter.ReportNowCount);
    }

    [Fact]
    public void ReceivedPlayLibrary_StartsTheLibraryOnTheLocalPlayer()
    {
        Guid mediaFileID = Guid.NewGuid();

        _hub.RaisePlayerCommandReceived(new() { Command = PlayerCommandEnum.PlayLibrary, MediaFileID = mediaFileID, Sort = LibrarySortEnum.Artist, Descending = true, Search = "abba" });

        Assert.Equal([$"StartLibrary {mediaFileID} Artist True abba"], _player.CallList);
        Assert.Equal(1, _reporter.ReportNowCount);
    }

    [Fact]
    public void ReceivedPlayPlaylist_StartsThePlaylistOnTheLocalPlayer_AndReports()
    {
        Guid playlistID = Guid.NewGuid();
        Guid entryID = Guid.NewGuid();

        _hub.RaisePlayerCommandReceived(new() { Command = PlayerCommandEnum.PlayPlaylist, PlaylistID = playlistID, EntryID = entryID });

        Assert.Equal([$"StartPlaylist {playlistID} {entryID}"], _player.CallList);
        Assert.Equal(1, _reporter.ReportNowCount);
    }

    [Theory]
    [InlineData(PlaybackRoleEnum.Controller)]
    [InlineData(PlaybackRoleEnum.Local)]
    public void AReceivedCommand_WhileNotActive_IsIgnored(PlaybackRoleEnum role)
    {
        _remote.Role = role;

        _hub.RaisePlayerCommandReceived(new() { Command = PlayerCommandEnum.Next });

        Assert.Empty(_player.CallList);
        Assert.Equal(0, _reporter.ReportNowCount);
        Assert.Contains(_logger.EntryList, x => x.Level == LogLevel.Information);
    }

    [Fact]
    public void ACommandThatFails_IsLoggedByTypeOnly_AndTheNextOneStillRuns()
    {
        _player.NextFailure = new InvalidOperationException("secret-detail");

        _hub.RaisePlayerCommandReceived(new() { Command = PlayerCommandEnum.Next });
        _hub.RaisePlayerCommandReceived(new() { Command = PlayerCommandEnum.Previous });

        Assert.Equal(["Next", "Previous"], _player.CallList);
        Assert.Contains(_logger.EntryList, x => x.Level == LogLevel.Warning && x.Message.Contains(nameof(InvalidOperationException)));
        Assert.DoesNotContain(_logger.EntryList, x => x.Message.Contains("secret"));
    }

    [Fact]
    public void Start_CalledTwice_SubscribesOnce()
    {
        _handler.Start();

        _hub.RaisePlayerCommandReceived(new() { Command = PlayerCommandEnum.Next });

        Assert.Equal(["Next"], _player.CallList);
    }

    #endregion -- Commands --

    #region -- Transfer --

    [Fact]
    public void TransferRequested_PausesAndReportsBeforeAnythingElse()
    {
        _player.Current = Item();
        _player.IsPlaying = true;
        bool wasPausedAtTheReport = false;
        _reporter.OnReportNow = () => wasPausedAtTheReport = !_player.IsPlaying;

        _hub.RaisePlayerTransferRequested();

        Assert.Equal(["Pause"], _player.CallList);
        Assert.True(wasPausedAtTheReport);
        Assert.Equal(1, _reporter.ReportNowCount);
    }

    [Fact]
    public void TransferRequested_ForADeviceThatIsNotActive_IsIgnored()
    {
        _remote.Role = PlaybackRoleEnum.Controller;

        _hub.RaisePlayerTransferRequested();

        Assert.Empty(_player.CallList);
        Assert.Equal(0, _reporter.ReportNowCount);
    }

    [Fact]
    public void AFailedTransfer_MakesTheDeviceThatStoppedForItPlayOn()
    {
        _player.Current = Item();
        _player.IsPlaying = true;
        _hub.RaisePlayerTransferRequested();
        _player.CallList.Clear();

        _remote.RaiseTransferFailed();

        Assert.Equal(["Resume"], _player.CallList);
        Assert.True(_player.IsPlaying);
    }

    [Fact]
    public void AFailedTransfer_DoesNotStartADeviceThatWasPausedBeforeIt()
    {
        _player.Current = Item();
        _player.IsPlaying = false;
        _hub.RaisePlayerTransferRequested();
        _player.CallList.Clear();

        _remote.RaiseTransferFailed();

        Assert.Empty(_player.CallList);
    }

    [Fact]
    public void StartRequested_AdoptsTheQueue_PlaysAtThePosition_ThenClaimsActive()
    {
        _remote.Role = PlaybackRoleEnum.Controller;
        QueueItem current = Item();
        ResponseQueue queue = Queue(8, current);
        bool playedAtTheClaim = false;
        TimeSpan positionAtTheClaim = TimeSpan.Zero;
        _queue.SnapshotList.Clear();
        _player.Current = current;
        _reporter.OnClaimNow = () =>
        {
            playedAtTheClaim = _player.IsPlaying;
            positionAtTheClaim = _player.Position;
        };

        _hub.RaisePlayerStartRequested(State(3, 40, 61.5), queue);

        Assert.Equal([(queue, false)], _queue.SnapshotList);
        Assert.Equal([$"SetVolume {0.4}", "PlayCurrent", $"Seek {TimeSpan.FromSeconds(61.5)}"], _player.CallList);
        Assert.True(playedAtTheClaim);
        Assert.Equal(TimeSpan.FromSeconds(61.5), positionAtTheClaim);
        Assert.Equal(1, _reporter.ClaimNowCount);
    }

    [Fact]
    public void StartRequested_AtThePositionZero_DoesNotSeek()
    {
        _player.Current = Item();

        _hub.RaisePlayerStartRequested(State(position: 0), Queue(1, _player.Current));

        Assert.Equal([$"SetVolume {0.5}", "PlayCurrent"], _player.CallList);
    }

    [Fact]
    public void StartRequested_WithAnEmptyQueue_StartsNothing()
    {
        _hub.RaisePlayerStartRequested(State(), Queue(1, null));

        Assert.Empty(_player.CallList);
        Assert.Empty(_queue.SnapshotList);
        Assert.Equal(0, _reporter.ClaimNowCount);
    }

    [Fact]
    public void StartRequested_WithAMissingFile_WaitsForTheDownload_ThenPlays_AndOnlyThenClaims()
    {
        TaskCompletionSource gate = new(TaskCreationOptions.RunContinuationsAsynchronously);
        _player.PlayCurrentGate = gate.Task;
        _player.Current = Item();

        _hub.RaisePlayerStartRequested(State(position: 5), Queue(1, _player.Current));

        Assert.Equal([$"SetVolume {0.5}", "PlayCurrent"], _player.CallList);
        Assert.Equal(0, _reporter.ClaimNowCount);

        gate.SetResult();
        WaitUntil(() => _reporter.ClaimNowCount == 1);

        Assert.True(_player.IsPlaying);
    }

    [Fact]
    public void StartRequested_WhenNothingCouldBePlayed_DoesNotClaim()
    {
        // The queue has an item but the player ends up with nothing playing (every file failed).
        _player.Current = null;

        _hub.RaisePlayerStartRequested(State(), Queue(1, Item()));

        Assert.Equal(0, _reporter.ClaimNowCount);
    }

    [Fact]
    public void StartRequested_TwiceForTheSameTransfer_StartsOnce()
    {
        _player.Current = Item();
        ResponseQueue queue = Queue(8, _player.Current);

        _hub.RaisePlayerStartRequested(State(3), queue);
        _hub.RaisePlayerStartRequested(State(3), queue);

        Assert.Equal(1, _reporter.ClaimNowCount);
        Assert.Single(_queue.SnapshotList);
        Assert.Single(_player.CallList, x => x == "PlayCurrent");
    }

    [Fact]
    public void StartRequested_ThatDidNotGetToPlay_CanBeTriedAgainWithTheSameState()
    {
        ResponseQueue queue = Queue(8, Item());
        _player.Current = null;

        _hub.RaisePlayerStartRequested(State(3), queue);
        _hub.RaisePlayerStartRequested(State(3), queue);

        Assert.Equal(2, _player.CallList.Count(x => x == "PlayCurrent"));
        Assert.Equal(0, _reporter.ClaimNowCount);
    }

    [Fact]
    public void StartRequested_ForALaterTransfer_StartsAgain()
    {
        _player.Current = Item();

        _hub.RaisePlayerStartRequested(State(3), Queue(8, _player.Current));
        _hub.RaisePlayerStartRequested(State(9), Queue(12, _player.Current));

        Assert.Equal(2, _reporter.ClaimNowCount);
    }

    [Fact]
    public void StartRequested_WhenTheClaimIsRefused_PausesAgain()
    {
        _reporter.ClaimResult = false;
        _player.Current = Item();

        _hub.RaisePlayerStartRequested(State(), Queue(1, _player.Current));

        Assert.False(_player.IsPlaying);
        Assert.Equal("Pause", _player.CallList.Last());
    }

    #endregion -- Transfer --

    #region -- The live session --

    private static RemoteState Remote(long revision, int volume, Guid? activeDeviceID = null)
    {
        return new(State(revision, volume, activeDeviceID: activeDeviceID ?? Guid.NewGuid()), DateTime.UtcNow);
    }

    [Fact]
    public void AController_AdoptsTheVolumeOfEachStatePush_Once()
    {
        _remote.Role = PlaybackRoleEnum.Controller;
        Guid active = Guid.NewGuid();
        _remote.Remote = Remote(1, 40, active);

        _remote.RaiseChanged();
        _remote.RaiseChanged();
        Assert.Equal([$"SetVolume {0.4}"], _player.CallList);

        _remote.Remote = Remote(2, 70, active);
        _remote.RaiseChanged();

        Assert.Equal([$"SetVolume {0.4}", $"SetVolume {0.7}"], _player.CallList);
    }

    [Fact]
    public void AControllerThatAlreadyHasThatVolume_DoesNothing()
    {
        _remote.Role = PlaybackRoleEnum.Controller;
        _player.Volume = 0.4;
        _remote.Remote = Remote(1, 40);

        _remote.RaiseChanged();

        Assert.Empty(_player.CallList);
    }

    [Fact]
    public void TheActiveDevice_NeverAdoptsAVolume_ItIsTheSource()
    {
        _remote.Role = PlaybackRoleEnum.Active;
        _remote.Remote = Remote(1, 10);

        _remote.RaiseChanged();

        Assert.Empty(_player.CallList);
    }

    [Fact]
    public void AControllerThatConnects_AdoptsTheSessionVolume_AndChangesNothingThatPlays()
    {
        _remote.Role = PlaybackRoleEnum.Controller;
        _player.Current = Item();
        _player.IsPlaying = false;
        _remote.Remote = Remote(1, 20);

        _remote.RaiseChanged();

        Assert.Equal([$"SetVolume {0.2}"], _player.CallList);
        Assert.Equal(0, _reporter.ReportNowCount);
        Assert.Equal(0, _reporter.ClaimNowCount);
    }

    [Fact]
    public void ADeviceThatLostTheActiveRole_WhilePlaying_Stops()
    {
        _remote.RaiseChanged();
        _player.Current = Item();
        _player.IsPlaying = true;

        _remote.Role = PlaybackRoleEnum.Controller;
        _remote.RaiseChanged();

        Assert.Equal(["Pause"], _player.CallList);
    }

    [Fact]
    public void ADeviceThatWasNeverActive_IsNotStoppedByTheHandler_TheClaimDecidesIt()
    {
        _remote.Role = PlaybackRoleEnum.Local;
        _player.Current = Item();
        _player.IsPlaying = true;
        _remote.RaiseChanged();

        _remote.Role = PlaybackRoleEnum.Controller;
        _remote.RaiseChanged();

        Assert.Empty(_player.CallList);
        Assert.True(_player.IsPlaying);
    }

    #endregion -- The live session --

    private static void WaitUntil(Func<bool> condition)
    {
        DateTime deadline = DateTime.UtcNow.AddSeconds(10);
        while (!condition())
        {
            Assert.True(DateTime.UtcNow < deadline, "Timed out waiting for the condition.");
            Thread.Sleep(10);
        }
    }
}
