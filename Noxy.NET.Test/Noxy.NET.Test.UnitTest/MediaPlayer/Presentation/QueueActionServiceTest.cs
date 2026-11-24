using Noxy.NET.MediaPlayer.Domain.Models;
using Noxy.NET.MediaPlayer.Domain.Responses.Remote;
using Noxy.NET.MediaPlayer.Presentation;
using Noxy.NET.MediaPlayer.Presentation.Models;
using Noxy.NET.MediaPlayer.Presentation.Services;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class QueueActionServiceTest
{
    private readonly FakePlaybackControlService _control = new();
    private readonly FakeQueueService _queue = new();
    private readonly QueueActionService _service;

    public QueueActionServiceTest()
    {
        _service = new(_control, _queue);
    }

    private static QueueItem Item()
    {
        return new(Guid.NewGuid(), Guid.NewGuid());
    }

    private void SetQueue(List<QueueItem> history, QueueItem? current, List<QueueItem> userQueue, List<QueueItem> source)
    {
        ResponseQueue response = new() { QueueRevision = 1, Source = null, HistoryList = history, Current = current, UserQueueList = userQueue, SourceList = source };
        _queue.Queue = LiveQueue.From(response, PresentationConstants.QueueHistoryLimit);
    }

    [Fact]
    public async Task Remove_RemovesEveryChosenItem_AndTheCurrentOneLast()
    {
        QueueItem played = Item(), current = Item(), queued = Item(), fromSource = Item();
        SetQueue([played], current, [queued], [fromSource]);

        BulkResult result = await _service.Remove([played.ItemID, current.ItemID, queued.ItemID, fromSource.ItemID]);

        Assert.Equal([$"RemoveFromQueue {played.ItemID}", $"RemoveFromQueue {queued.ItemID}", $"RemoveFromQueue {fromSource.ItemID}", $"RemoveFromQueue {current.ItemID}"], _control.CallList);
        Assert.Equal(4, result.SucceededCount);
        Assert.Empty(result.FailedIDList);
    }

    [Fact]
    public async Task Remove_ACallThatThrows_IsAFailureAndTheRestStillRun()
    {
        QueueItem first = Item(), second = Item();
        SetQueue([], Item(), [first, second], []);
        _control.RemoveFromQueueHandler = id =>
        {
            if (id == first.ItemID) throw new InvalidOperationException("Gone.");
        };

        BulkResult result = await _service.Remove([first.ItemID, second.ItemID]);

        Assert.Equal([first.ItemID], result.FailedIDList);
        Assert.Equal(1, result.SucceededCount);
        Assert.Equal([$"RemoveFromQueue {first.ItemID}", $"RemoveFromQueue {second.ItemID}"], _control.CallList);
    }

    [Fact]
    public async Task PlayNext_MovesUserQueueItemsToTheFrontLastFirst()
    {
        QueueItem a = Item(), b = Item(), c = Item();
        SetQueue([], Item(), [a, b, c], []);

        BulkResult result = await _service.PlayNext([a.ItemID, c.ItemID]);

        Assert.Equal([$"MoveInQueue {c.ItemID} ", $"MoveInQueue {a.ItemID} "], _control.CallList);
        Assert.Equal(2, result.SucceededCount);
    }

    [Fact]
    public async Task PlayNext_OfASourceItem_MovesItToTheFront_WithoutAddingANewOne()
    {
        QueueItem fromSource = Item();
        SetQueue([], Item(), [], [fromSource]);

        await _service.PlayNext([fromSource.ItemID]);

        Assert.Equal([$"MoveInQueue {fromSource.ItemID} "], _control.CallList);
    }

    [Fact]
    public async Task PlayNext_OfUserQueueAndSourceItemsTogether_KeepsTheOrderOfTheQueue()
    {
        QueueItem queued = Item(), first = Item(), second = Item();
        SetQueue([], Item(), [queued], [first, second]);

        await _service.PlayNext([queued.ItemID, second.ItemID]);

        Assert.Equal([$"MoveInQueue {second.ItemID} ", $"MoveInQueue {queued.ItemID} "], _control.CallList);
    }

    [Fact]
    public async Task PlayNext_KeepsTheOrderOfTheQueueWhateverTheOrderAsked()
    {
        QueueItem a = Item(), b = Item();
        SetQueue([], Item(), [a, b], []);

        await _service.PlayNext([b.ItemID, a.ItemID]);

        Assert.Equal([$"MoveInQueue {b.ItemID} ", $"MoveInQueue {a.ItemID} "], _control.CallList);
    }

    [Fact]
    public async Task PlayNext_SkipsHistoryAndTheCurrentOne()
    {
        QueueItem played = Item(), current = Item();
        SetQueue([played], current, [], []);

        BulkResult result = await _service.PlayNext([played.ItemID, current.ItemID]);

        Assert.Empty(_control.CallList);
        Assert.Equal(0, result.SucceededCount);
        Assert.Empty(result.FailedIDList);
    }
}
