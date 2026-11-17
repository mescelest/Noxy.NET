using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Noxy.NET.MediaPlayer.Application.Configurations;
using Noxy.NET.MediaPlayer.Application.Notifications.Remote;
using Noxy.NET.MediaPlayer.Application.Requests.Remote;
using Noxy.NET.MediaPlayer.Application.Services;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Models;
using Noxy.NET.MediaPlayer.Domain.Responses.Remote;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Remote;

public class LiveSessionServiceTransferTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();

    private readonly FakePublisher _publisher = new();
    private readonly FakeTimeProvider _time = new();
    private readonly ConfigRemote _config = new() { ActiveGraceSeconds = 10, IdleTeardownMinutes = 10, HistoryLimit = 50, MaxQueueLength = 100, TransferTimeoutSeconds = 5 };
    private readonly DeviceRegistryService _registry;
    private readonly LiveSessionService _service;

    public LiveSessionServiceTransferTest()
    {
        _registry = new(_publisher, Options.Create(_config));
        _service = new(_registry, Options.Create(_config), _time, _publisher, new FakeUnitOfWorkFactory(new()), NullLogger<LiveSessionService>.Instance);
    }

    private async Task<Guid> Device(Guid? userID = null)
    {
        Guid deviceID = Guid.CreateVersion7();
        await _registry.Register(userID ?? UserID, new() { DeviceID = deviceID, Name = "Device" }, $"c-{deviceID}");
        return deviceID;
    }

    private static RequestReportPlayerState Report(long revision, bool playing = true, double position = 5)
    {
        return new() { StateRevision = revision, IsPlaying = playing, PositionSeconds = position, Volume = 40, Shuffle = ShuffleModeEnum.Off, Repeat = RepeatModeEnum.No };
    }

    private static ResponseQueue QueueOf(QueueItem current)
    {
        return new() { QueueRevision = 0, Source = null, HistoryList = [], Current = current, UserQueueList = [], SourceList = [new(Guid.CreateVersion7(), Guid.CreateVersion7())] };
    }

    private static RequestClaimActive ClaimRequest(QueueItem current, bool playing = true, long revision = 1)
    {
        return new() { State = Report(revision, playing), Queue = QueueOf(current) };
    }

    private async Task<(Guid Active, QueueItem Current)> ActiveSession(bool playing = true)
    {
        Guid active = await Device();
        QueueItem current = new(Guid.CreateVersion7(), Guid.CreateVersion7());
        Assert.True(await _service.Claim(UserID, active, ClaimRequest(current, playing)));
        _publisher.PublishedList.Clear();
        return (active, current);
    }

    private static async Task Eventually(Func<bool> condition)
    {
        bool met = condition();
        for (int attempt = 0; attempt < 500 && !met; attempt++)
        {
            await Task.Delay(10, TestContext.Current.CancellationToken);
            met = condition();
        }

        Assert.True(met);
    }

    private Task<bool> Transfer(FakeTransferDevices devices, Guid target)
    {
        return _service.Transfer(UserID, target, devices.RequestFinalState, devices.Start);
    }

    [Fact]
    public async Task Transfer_AsksTheActiveDeviceForAFinalState_ThenStartsTheTarget_WithThatStateAndTheQueue()
    {
        (Guid active, QueueItem current) = await ActiveSession();
        Guid target = await Device();
        FakeTransferDevices devices = new() { OnFinalStateRequested = _ => _service.Report(UserID, active, Report(2, false, 42)) };

        Assert.True(await Transfer(devices, target));

        await Eventually(() => devices.StartList.Count == 1);
        (Guid started, ResponsePlayerState state, ResponseQueue queue) = devices.StartList[0];
        Assert.Equal(active, Assert.Single(devices.FinalStateRequestList));
        Assert.Equal(target, started);
        Assert.Equal(42, state.PositionSeconds);
        Assert.False(state.IsPlaying);
        Assert.Equal(current, queue.Current);
    }

    [Fact]
    public async Task Transfer_Completed_MakesTheTargetActive_ThroughTheClaimRule()
    {
        (Guid active, QueueItem current) = await ActiveSession();
        Guid target = await Device();
        FakeTransferDevices devices = new()
        {
            OnFinalStateRequested = _ => _service.Report(UserID, active, Report(2, false, 42)),
            OnStart = async (device, state, queue) => Assert.True(await _service.Claim(UserID, device, new() { State = Report(1, true, state.PositionSeconds), Queue = queue }))
        };

        await Transfer(devices, target);

        await Eventually(() => _service.GetActiveDeviceID(UserID) == target);
        Assert.Equal(current, _service.GetQueue(UserID)?.Current);
        Assert.Empty(_publisher.GetList<NotificationTransferFailed>());
    }

    [Fact]
    public async Task Transfer_WhenTheOldDeviceNeverAnswers_UsesTheLastStateAdvancedByTheElapsedTime_WhenPlaying()
    {
        await ActiveSession();
        Guid target = await Device();
        FakeTransferDevices devices = new();

        await Transfer(devices, target);
        _time.Advance(TimeSpan.FromSeconds(_config.TransferTimeoutSeconds));

        await Eventually(() => devices.StartList.Count == 1);
        Assert.Equal(10, devices.StartList[0].State.PositionSeconds);
    }

    [Fact]
    public async Task Transfer_WhenTheOldDeviceNeverAnswers_KeepsTheLastStateUnchanged_WhenPaused()
    {
        await ActiveSession(false);
        Guid target = await Device();
        FakeTransferDevices devices = new();

        await Transfer(devices, target);
        _time.Advance(TimeSpan.FromSeconds(_config.TransferTimeoutSeconds));

        await Eventually(() => devices.StartList.Count == 1);
        Assert.Equal(5, devices.StartList[0].State.PositionSeconds);
    }

    [Fact]
    public async Task Transfer_ToAnOfflineOrUnknownDevice_IsRefused_AndChangesNothing()
    {
        (Guid active, _) = await ActiveSession();
        Guid offline = await Device();
        await _registry.Disconnect($"c-{offline}");
        FakeTransferDevices devices = new();

        Assert.False(await Transfer(devices, offline));
        Assert.False(await Transfer(devices, Guid.CreateVersion7()));

        Assert.Empty(devices.FinalStateRequestList);
        Assert.Equal(active, _service.GetActiveDeviceID(UserID));
    }

    [Fact]
    public async Task Transfer_ToTheActiveDevice_IsRefused()
    {
        (Guid active, _) = await ActiveSession();
        FakeTransferDevices devices = new();

        Assert.False(await Transfer(devices, active));

        Assert.Empty(devices.FinalStateRequestList);
    }

    [Fact]
    public async Task Transfer_WithNoSession_IsRefused()
    {
        Guid target = await Device();
        FakeTransferDevices devices = new();

        Assert.False(await Transfer(devices, target));
    }

    [Fact]
    public async Task Transfer_WhileAnotherIsPending_IsRefused()
    {
        await ActiveSession();
        Guid target = await Device();
        Guid other = await Device();
        FakeTransferDevices devices = new();

        Assert.True(await Transfer(devices, target));
        Assert.False(await Transfer(devices, other));

        Assert.Single(devices.FinalStateRequestList);
    }

    [Fact]
    public async Task Transfer_WhenTheTargetNeverClaims_PushesTransferFailed_AndTheOldDeviceStaysActive()
    {
        (Guid active, _) = await ActiveSession();
        Guid target = await Device();
        FakeTransferDevices devices = new() { OnFinalStateRequested = _ => _service.Report(UserID, active, Report(2, false, 42)) };

        await Transfer(devices, target);
        await Eventually(() => devices.StartList.Count == 1);
        await Eventually(() => _time.ActiveTimerCount > 0);
        _time.Advance(TimeSpan.FromSeconds(_config.TransferTimeoutSeconds));

        await Eventually(() => _publisher.GetList<NotificationTransferFailed>().Count == 1);
        Assert.Equal(UserID, _publisher.GetList<NotificationTransferFailed>()[0].UserID);
        Assert.Equal(active, _service.GetActiveDeviceID(UserID));
    }

    [Fact]
    public async Task ThePendingFlag_IsClearedAfterEveryOutcome()
    {
        (Guid active, _) = await ActiveSession();
        Guid target = await Device();
        FakeTransferDevices failing = new() { OnFinalStateRequested = _ => _service.Report(UserID, active, Report(2, false, 42)) };

        await Transfer(failing, target);
        await Eventually(() => failing.StartList.Count == 1);
        await Eventually(() => _time.ActiveTimerCount > 0);
        _time.Advance(TimeSpan.FromSeconds(_config.TransferTimeoutSeconds));
        await Eventually(() => _publisher.GetList<NotificationTransferFailed>().Count == 1);

        await Eventually(() => Transfer(new(), target).GetAwaiter().GetResult());
    }

    [Fact]
    public async Task Transfer_OfAnotherAccount_NeverTouchesThisAccountsSession()
    {
        (Guid active, _) = await ActiveSession();
        Guid other = Guid.CreateVersion7();
        Guid otherDevice = await Device(other);
        FakeTransferDevices devices = new();

        Assert.False(await _service.Transfer(other, otherDevice, devices.RequestFinalState, devices.Start));
        Assert.False(await _service.Transfer(other, active, devices.RequestFinalState, devices.Start));

        Assert.Empty(devices.FinalStateRequestList);
        Assert.Equal(active, _service.GetActiveDeviceID(UserID));
    }

    [Fact]
    public async Task Transfer_WhenTheOldDeviceCannotBeReached_UsesTheLastState_AndStartsTheTarget()
    {
        await ActiveSession();
        Guid target = await Device();
        FakeTransferDevices devices = new() { OnFinalStateRequested = _ => throw new InvalidOperationException("The device is offline.") };

        Assert.True(await Transfer(devices, target));

        await Eventually(() => devices.StartList.Count == 1);
        Assert.False(devices.StartList[0].State.IsPlaying);
        Assert.Equal(5, devices.StartList[0].State.PositionSeconds);
    }

    [Fact]
    public async Task Transfer_WhenTheTargetCannotBeReached_PushesTransferFailed_AndTheOldDeviceStaysActive()
    {
        (Guid active, _) = await ActiveSession();
        Guid target = await Device();
        FakeTransferDevices devices = new()
        {
            OnFinalStateRequested = _ => _service.Report(UserID, active, Report(2, false, 42)),
            OnStart = (_, _, _) => throw new InvalidOperationException("The device is offline.")
        };

        await Transfer(devices, target);

        await Eventually(() => _publisher.GetList<NotificationTransferFailed>().Count == 1);
        Assert.Equal(active, _service.GetActiveDeviceID(UserID));
        await Eventually(() => Transfer(new(), target).GetAwaiter().GetResult());
    }

    [Fact]
    public async Task Transfer_ARoutineReportWhilePlaying_DoesNotEndTheWait()
    {
        (Guid active, _) = await ActiveSession();
        Guid target = await Device();
        FakeTransferDevices devices = new() { OnFinalStateRequested = _ => _service.Report(UserID, active, Report(2, true, 9)) };

        await Transfer(devices, target);

        Assert.Empty(devices.StartList);
        _time.Advance(TimeSpan.FromSeconds(_config.TransferTimeoutSeconds));
        await Eventually(() => devices.StartList.Count == 1);
        Assert.False(devices.StartList[0].State.IsPlaying);
    }

    [Fact]
    public async Task Transfer_TheTargetsClaim_IsAccepted_EvenWhenALateReportMadeTheOldDeviceLookPlayingAgain()
    {
        (Guid active, _) = await ActiveSession();
        Guid target = await Device();
        bool claimed = false;
        FakeTransferDevices devices = new()
        {
            OnFinalStateRequested = _ => _service.Report(UserID, active, Report(2, false, 42)),
            OnStart = async (device, state, queue) =>
            {
                await _service.Report(UserID, active, Report(3, true, 43));
                claimed = await _service.Claim(UserID, device, new() { State = Report(1, true, state.PositionSeconds), Queue = queue });
            }
        };

        await Transfer(devices, target);

        await Eventually(() => claimed);
        Assert.Equal(target, _service.GetActiveDeviceID(UserID));
        Assert.Empty(_publisher.GetList<NotificationTransferFailed>());
    }
}
