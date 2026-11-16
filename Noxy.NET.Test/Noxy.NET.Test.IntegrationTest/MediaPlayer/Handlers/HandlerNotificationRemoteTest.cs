using Noxy.NET.MediaPlayer.API.Handlers.Remote;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Models;
using Noxy.NET.MediaPlayer.Domain.Responses.Remote;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Handlers;

public class HandlerNotificationRemoteTest
{
    private static readonly Guid UserID = Guid.CreateVersion7();
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    private readonly FakeHubContext _hub = new();

    private void AssertPushed(MediaHubEventEnum eventKind, params object?[] expectedArgumentList)
    {
        (string userID, string method, object?[] argumentList, CancellationToken token) = Assert.Single(_hub.SendList);
        Assert.Equal(UserID.ToString(), userID);
        Assert.Equal(eventKind.ToString(), method);
        Assert.Equal(expectedArgumentList, argumentList);
        Assert.Equal(Token, token);
    }

    [Fact]
    public async Task DeviceListChanged_PushesTheListToThatUserOnly()
    {
        List<ResponseDevice> list = [new() { DeviceID = Guid.CreateVersion7(), Name = "Laptop", IsActive = true }];

        await new HandlerNotificationDeviceListChanged(_hub, new FakeLogger<HandlerNotificationDeviceListChanged>()).Handle(new(UserID, list), Token);

        AssertPushed(MediaHubEventEnum.DeviceListChanged, list);
    }

    [Fact]
    public async Task PlayerStateChanged_PushesTheStateToThatUserOnly()
    {
        ResponsePlayerState state = new()
        {
            ActiveDeviceID = null,
            StateRevision = 1,
            IsPlaying = false,
            PositionSeconds = 0,
            ServerTime = DateTime.UtcNow,
            Volume = 10,
            Shuffle = ShuffleModeEnum.Off,
            Repeat = RepeatModeEnum.No,
            CurrentItemID = null,
            Title = null,
            Artist = null
        };

        await new HandlerNotificationPlayerStateChanged(_hub, new FakeLogger<HandlerNotificationPlayerStateChanged>()).Handle(new(UserID, state), Token);

        AssertPushed(MediaHubEventEnum.PlayerStateChanged, state);
    }

    [Fact]
    public async Task QueueChanged_PushesTheDeltaToThatUserOnly()
    {
        ResponseQueueDelta delta = new() { QueueRevision = 2, Edit = QueueEditEnum.Advance };

        await new HandlerNotificationQueueChanged(_hub, new FakeLogger<HandlerNotificationQueueChanged>()).Handle(new(UserID, delta), Token);

        AssertPushed(MediaHubEventEnum.QueueChanged, delta);
    }

    [Fact]
    public async Task QueueSnapshot_PushesTheQueueToThatUserOnly()
    {
        ResponseQueue queue = new() { QueueRevision = 1, Source = null, HistoryList = [], Current = new QueueItem(Guid.CreateVersion7(), Guid.CreateVersion7()), UserQueueList = [], SourceList = [] };

        await new HandlerNotificationQueueSnapshot(_hub, new FakeLogger<HandlerNotificationQueueSnapshot>()).Handle(new(UserID, queue), Token);

        AssertPushed(MediaHubEventEnum.QueueSnapshot, queue);
    }

    [Fact]
    public async Task SessionEnded_PushesTheEventWithNoPayloadToThatUserOnly()
    {
        await new HandlerNotificationSessionEnded(_hub, new FakeLogger<HandlerNotificationSessionEnded>()).Handle(new(UserID), Token);

        (string userID, string method, object?[] argumentList, _) = Assert.Single(_hub.SendList);
        Assert.Equal(UserID.ToString(), userID);
        Assert.Equal(nameof(MediaHubEventEnum.SessionEnded), method);
        Assert.Empty(argumentList);
    }

    [Fact]
    public async Task TransferFailed_PushesTheEventWithNoPayloadToThatUserOnly()
    {
        await new HandlerNotificationTransferFailed(_hub, new FakeLogger<HandlerNotificationTransferFailed>()).Handle(new(UserID), Token);

        (string userID, string method, object?[] argumentList, _) = Assert.Single(_hub.SendList);
        Assert.Equal(UserID.ToString(), userID);
        Assert.Equal(nameof(MediaHubEventEnum.TransferFailed), method);
        Assert.Empty(argumentList);
    }

    [Fact]
    public async Task EveryRemoteHandler_WhenThePushThrows_DoesNotThrowAndLogsAWarning()
    {
        _hub.Failure = new InvalidOperationException("The connection is gone.");
        FakeLogger<HandlerNotificationSessionEnded> logger = new();

        await new HandlerNotificationSessionEnded(_hub, logger).Handle(new(UserID), Token);

        Assert.Empty(_hub.SendList);
        Assert.Contains(logger.EntryList, x => x.Level == Microsoft.Extensions.Logging.LogLevel.Warning);
    }
}
