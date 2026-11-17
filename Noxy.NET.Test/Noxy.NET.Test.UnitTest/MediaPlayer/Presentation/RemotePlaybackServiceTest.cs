using Microsoft.Extensions.Logging;
using Noxy.NET.MediaPlayer.Application.Requests.Remote;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Responses.Remote;
using Noxy.NET.MediaPlayer.Presentation.Enums;
using Noxy.NET.MediaPlayer.Presentation.Services;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class RemotePlaybackServiceTest
{
    private readonly FakeMediaHubService _hub = new() { IsConnected = true };
    private readonly FakeDeviceStore _storeDevice = new();
    private readonly FakeQueueService _queue = new();
    private readonly FakeTimeProvider _time = new();
    private readonly FakeLogger<RemotePlaybackService> _logger = new();
    private readonly DeviceService _serviceDevice;
    private readonly RemotePlaybackService _service;
    private int _changedCount;

    public RemotePlaybackServiceTest()
    {
        _serviceDevice = new(_storeDevice);
        _service = new(_hub, _serviceDevice, _queue, _time, _logger);
        _service.Changed += () => _changedCount++;
    }

    private static ResponseDevice Device(Guid id, string name, bool isActive)
    {
        return new() { DeviceID = id, Name = name, IsActive = isActive };
    }

    private static ResponsePlayerState State(Guid? activeDeviceID, long revision, bool isPlaying = true, double position = 10, Guid? itemID = null)
    {
        return new()
        {
            ActiveDeviceID = activeDeviceID, StateRevision = revision, IsPlaying = isPlaying, PositionSeconds = position, ServerTime = DateTime.UtcNow, Volume = 50, Shuffle = ShuffleModeEnum.Off, Repeat = RepeatModeEnum.No, CurrentItemID = itemID,
            Title = "Title", Artist = null
        };
    }

    private static ResponseQueue Queue(long revision)
    {
        return new() { QueueRevision = revision, Source = null, HistoryList = [], Current = null, UserQueueList = [], SourceList = [] };
    }

    private async Task<Guid> Connect(params ResponseDevice[] otherDeviceList)
    {
        Guid me = await _serviceDevice.GetDeviceID();
        _hub.InvokeHandler = (method, _) => method == MediaHubMethodEnum.RegisterDevice ? (List<ResponseDevice>)[Device(me, "Me", false), .. otherDeviceList] : null;
        _hub.RaiseConnected();

        return me;
    }

    #region -- Registering --

    [Fact]
    public async Task Register_SendsTheDeviceIDAndName_OnEveryConnection()
    {
        Guid me = await Connect();
        await _serviceDevice.SetName("Kitchen");
        _hub.RaiseConnected();

        List<RequestRegisterDevice> requestList = _hub.ArgumentList(MediaHubMethodEnum.RegisterDevice).Cast<RequestRegisterDevice>().ToList();
        Assert.Equal(2, requestList.Count);
        Assert.All(requestList, x => Assert.Equal(me, x.DeviceID));
        Assert.Equal(Environment.MachineName, requestList[0].Name);
        Assert.Equal("Kitchen", requestList[1].Name);
    }

    [Fact]
    public async Task Registered_IsRaisedWhenRegisteringFinished_ButNotWhenTheHubIsNotUp()
    {
        int registered = 0;
        _service.Registered += () => registered++;
        await Connect();
        Assert.Equal(1, registered);

        _hub.IsConnected = false;
        _hub.InvokeHandler = (_, _) => null;
        await _service.Register();

        Assert.Equal(1, registered);
    }

    [Fact]
    public async Task Register_StoresTheDeviceList_AndWhoIsActive()
    {
        Guid other = Guid.NewGuid();
        await Connect(Device(other, "Phone", true));

        Assert.Equal(2, _service.DeviceList.Count);
        Assert.Equal(other, _service.ActiveDeviceID);
        Assert.True(_service.HasSession);
        Assert.Equal(PlaybackRoleEnum.Controller, _service.Role);
        Assert.True(_changedCount > 0);
    }

    [Fact]
    public async Task Register_WhenTheHubIsNotUp_ChangesNothing()
    {
        _hub.IsConnected = false;

        await _service.Register();

        Assert.Empty(_service.DeviceList);
        Assert.Equal(PlaybackRoleEnum.Local, _service.Role);
    }

    [Fact]
    public async Task Register_WhenTheServerRefuses_IsLoggedByTypeOnly_AndTheLaterStepsStillRun()
    {
        _hub.InvokeHandler = (_, _) => throw new InvalidOperationException("secret detail");
        _hub.RaiseConnected();
        Guid other = Guid.NewGuid();

        _hub.RaiseDeviceListChanged([Device(other, "Phone", true)]);

        Assert.Equal(other, _service.ActiveDeviceID);
        Assert.Contains(_logger.EntryList, x => x.Level == LogLevel.Error);
        Assert.DoesNotContain(_logger.EntryList, x => x.Message.Contains("secret"));
        await Task.CompletedTask;
    }

    [Fact]
    public async Task ADeviceListPush_ReplacesTheList()
    {
        Guid me = await Connect();
        Guid other = Guid.NewGuid();

        _hub.RaiseDeviceListChanged([Device(me, "Me", false), Device(other, "Phone", true)]);

        Assert.Equal(other, _service.ActiveDeviceID);
        Assert.Equal(2, _service.DeviceList.Count);
    }

    #endregion -- Registering --

    #region -- State --

    [Fact]
    public async Task AStatePush_ReplacesRemote_AndRaisesChanged()
    {
        Guid other = Guid.NewGuid();
        await Connect();
        int before = _changedCount;

        _hub.RaisePlayerStateChanged(State(other, 5, position: 42));

        Assert.Equal(42, _service.Remote?.State.PositionSeconds);
        Assert.Equal(_time.Now.UtcDateTime, _service.Remote?.ReceivedAt);
        Assert.True(_changedCount > before);
        Assert.True(_service.HasSession);
    }

    [Fact]
    public async Task AStalePush_WithALowerRevisionFromTheSameDevice_IsDropped()
    {
        Guid other = Guid.NewGuid();
        await Connect();
        _hub.RaisePlayerStateChanged(State(other, 5, position: 42));
        int before = _changedCount;

        _hub.RaisePlayerStateChanged(State(other, 4, position: 1));

        Assert.Equal(42, _service.Remote?.State.PositionSeconds);
        Assert.Equal(before, _changedCount);
    }

    [Fact]
    public async Task AStatePush_FromANewActiveDevice_IsAcceptedWhateverItsRevision()
    {
        Guid first = Guid.NewGuid();
        Guid second = Guid.NewGuid();
        await Connect();
        _hub.RaisePlayerStateChanged(State(first, 50, position: 42));

        _hub.RaisePlayerStateChanged(State(second, 1, position: 7));

        Assert.Equal(7, _service.Remote?.State.PositionSeconds);
        Assert.Equal(second, _service.ActiveDeviceID);
    }

    [Fact]
    public async Task Role_IsControllerWhenAnotherDeviceIsActive_ActiveWhenThisOne_LocalOtherwise_AndLocalWhenOffline()
    {
        Guid me = await Connect();
        Assert.Equal(PlaybackRoleEnum.Local, _service.Role);

        _hub.RaisePlayerStateChanged(State(Guid.NewGuid(), 1));
        Assert.Equal(PlaybackRoleEnum.Controller, _service.Role);

        _hub.RaisePlayerStateChanged(State(me, 1));
        Assert.Equal(PlaybackRoleEnum.Active, _service.Role);

        _hub.IsConnected = false;
        Assert.Equal(PlaybackRoleEnum.Local, _service.Role);
    }

    [Fact]
    public async Task MarkActive_MakesThisDeviceActiveAtOnce()
    {
        await Connect();

        _service.MarkActive();

        Assert.Equal(PlaybackRoleEnum.Active, _service.Role);
        Assert.True(_service.HasSession);
    }

    [Fact]
    public async Task Disconnected_ClearsTheSession_AndTheRoleIsLocal()
    {
        Guid other = Guid.NewGuid();
        await Connect(Device(other, "Phone", true));
        _hub.RaisePlayerStateChanged(State(other, 1));

        _hub.IsConnected = false;
        _hub.RaiseDisconnected();

        Assert.Null(_service.Remote);
        Assert.Null(_service.ActiveDeviceID);
        Assert.False(_service.HasSession);
        Assert.Equal(PlaybackRoleEnum.Local, _service.Role);
    }

    [Fact]
    public async Task SessionEnded_ClearsRemoteAndTheSession_ButKeepsTheLocalQueue()
    {
        Guid other = Guid.NewGuid();
        await Connect();
        _hub.RaisePlayerStateChanged(State(other, 1));
        int before = _changedCount;

        _hub.RaiseSessionEnded();

        Assert.Null(_service.Remote);
        Assert.False(_service.HasSession);
        Assert.Null(_service.ActiveDeviceID);
        Assert.Equal(PlaybackRoleEnum.Local, _service.Role);
        Assert.True(_changedCount > before);
        Assert.Empty(_queue.SnapshotList);
        Assert.Equal(0, _queue.ClearCount);
    }

    [Fact]
    public async Task ATransferFailedPush_IsForwarded()
    {
        int failed = 0;
        _service.TransferFailed += () => failed++;

        _hub.RaiseTransferFailed();

        Assert.Equal(1, failed);
        await Task.CompletedTask;
    }

    #endregion -- State --

    #region -- The queue --

    [Fact]
    public async Task AQueueChangedPush_IsHandedToTheQueueService()
    {
        await Connect();
        ResponseQueueDelta delta = new() { QueueRevision = 3, Edit = QueueEditEnum.Advance };

        _hub.RaiseQueueChanged(delta);

        Assert.Equal([delta], _queue.DeltaList);
    }

    [Fact]
    public async Task Deltas_ReachTheQueueService_InTheOrderTheyArrived()
    {
        await Connect();

        for (int revision = 1; revision <= 20; revision++) _hub.RaiseQueueChanged(new() { QueueRevision = revision, Edit = QueueEditEnum.Advance });

        Assert.Equal(Enumerable.Range(1, 20).Select(x => (long)x), _queue.DeltaList.Select(x => x.QueueRevision));
    }

    [Fact]
    public async Task AGap_RequestsASnapshot_AndHandsItToTheQueueService()
    {
        await Connect();
        ResponseQueue snapshot = Queue(9);
        _hub.InvokeHandler = (method, _) => method == MediaHubMethodEnum.RequestSnapshot ? snapshot : null;

        _queue.RaiseGapDetected();

        Assert.Single(_hub.ArgumentList(MediaHubMethodEnum.RequestSnapshot));
        Assert.Equal([(snapshot, false)], _queue.SnapshotList);
    }

    [Fact]
    public async Task ASnapshot_OnAController_ReplacesTheCopy_AndOnTheActiveDevice_KeepsItsPointer()
    {
        Guid me = await Connect();
        _hub.RaisePlayerStateChanged(State(Guid.NewGuid(), 1));
        ResponseQueue forController = Queue(1);
        _hub.RaiseQueueSnapshot(forController);

        _hub.RaisePlayerStateChanged(State(me, 1));
        ResponseQueue forActive = Queue(2);
        _hub.RaiseQueueSnapshot(forActive);

        Assert.Equal([(forController, false), (forActive, true)], _queue.SnapshotList);
    }

    [Fact]
    public async Task TheQueueOfASessionSomebodyPlays_PushedWhileRegistering_IsTakenOnceTheAnswerSaysSo()
    {
        Guid me = await _serviceDevice.GetDeviceID();
        Guid other = Guid.NewGuid();
        ResponseQueue sessionQueue = Queue(4);
        _hub.InvokeHandler = (method, _) =>
        {
            if (method != MediaHubMethodEnum.RegisterDevice) return null;

            _hub.RaiseQueueSnapshot(sessionQueue);
            _hub.RaisePlayerStateChanged(State(other, 3));
            return (List<ResponseDevice>)[Device(me, "Me", false), Device(other, "Phone", true)];
        };

        _hub.RaiseConnected();

        Assert.Equal([(sessionQueue, false)], _queue.SnapshotList);
    }

    [Fact]
    public async Task TheQueueOfASessionNobodyPlays_PushedWhileRegistering_IsNotTaken_SoThisDeviceKeepsItsOwnCopy()
    {
        Guid me = await _serviceDevice.GetDeviceID();
        _hub.InvokeHandler = (method, _) =>
        {
            if (method != MediaHubMethodEnum.RegisterDevice) return null;

            _hub.RaiseQueueSnapshot(Queue(4));
            return (List<ResponseDevice>)[Device(me, "Me", false)];
        };

        _hub.RaiseConnected();

        Assert.Empty(_queue.SnapshotList);
    }

    #endregion -- The queue --

    #region -- Position --

    [Fact]
    public async Task GetPosition_AdvancesWhilePlaying_AndIsFrozenWhenPaused()
    {
        await Connect();
        _hub.RaisePlayerStateChanged(State(Guid.NewGuid(), 1));

        _time.Now = _time.Now.AddSeconds(5);
        Assert.Equal(TimeSpan.FromSeconds(15), _service.GetPosition());

        _hub.RaisePlayerStateChanged(State(_service.ActiveDeviceID, 2, false, 20));
        _time.Now = _time.Now.AddSeconds(30);
        Assert.Equal(TimeSpan.FromSeconds(20), _service.GetPosition());
    }

    [Fact]
    public async Task GetPosition_WithNoRemoteState_IsZero()
    {
        await Connect();

        Assert.Equal(TimeSpan.Zero, _service.GetPosition());
    }

    [Fact]
    public async Task GetPosition_NeverGoesBackwardsBetweenReports()
    {
        Guid other = Guid.NewGuid();
        Guid item = Guid.NewGuid();
        await Connect();
        _hub.RaisePlayerStateChanged(State(other, 1, true, 10, item));
        _time.Now = _time.Now.AddSeconds(5);
        Assert.Equal(TimeSpan.FromSeconds(15), _service.GetPosition());

        // The next report was made a moment before it arrived: it is behind what was shown.
        _hub.RaisePlayerStateChanged(State(other, 2, true, 14.2, item));

        Assert.True(_service.GetPosition() >= TimeSpan.FromSeconds(15));
    }

    [Fact]
    public async Task GetPosition_FollowsASeekBack_ThatIsMoreThanADelay()
    {
        Guid other = Guid.NewGuid();
        Guid item = Guid.NewGuid();
        await Connect();
        _hub.RaisePlayerStateChanged(State(other, 1, true, 100, item));
        _time.Now = _time.Now.AddSeconds(5);
        Assert.Equal(TimeSpan.FromSeconds(105), _service.GetPosition());

        _hub.RaisePlayerStateChanged(State(other, 2, true, 20, item));

        Assert.Equal(TimeSpan.FromSeconds(20), _service.GetPosition());
    }

    [Fact]
    public async Task GetPosition_FollowsANewItem_EvenWhenItStartsBehind()
    {
        Guid other = Guid.NewGuid();
        await Connect();
        _hub.RaisePlayerStateChanged(State(other, 1, true, 100, Guid.NewGuid()));
        _time.Now = _time.Now.AddSeconds(1);
        Assert.Equal(TimeSpan.FromSeconds(101), _service.GetPosition());

        _hub.RaisePlayerStateChanged(State(other, 2, true, 0, Guid.NewGuid()));

        Assert.Equal(TimeSpan.Zero, _service.GetPosition());
    }

    #endregion -- Position --
}
