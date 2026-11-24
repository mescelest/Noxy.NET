using Noxy.NET.MediaPlayer.Domain.Models;
using Noxy.NET.MediaPlayer.Domain.Responses.Remote;
using Noxy.NET.MediaPlayer.Presentation.Extensions;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;
using Noxy.NET.MediaPlayer.Presentation.Models;

namespace Noxy.NET.MediaPlayer.Presentation.Services;

public class QueueActionService(IPlaybackControlService serviceControl, IQueueService serviceQueue) : IQueueActionService
{
    // Every chosen item leaves, the played ones and the upcoming ones first and the playing one last: removing it makes the device skip to what is left, or stop when nothing is, so it is done when the rest is gone and the player does not start songs that are about to be removed.
    public async Task<BulkResult> Remove(List<Guid> itemIDList)
    {
        HashSet<Guid> chosenSet = [.. itemIDList];
        ResponseQueue queue = serviceQueue.GetSnapshot();
        List<QueueItem> otherList = [.. queue.HistoryList.Concat(queue.UserQueueList).Concat(queue.SourceList).Where(x => chosenSet.Contains(x.ItemID))];
        List<QueueItem> removeList = queue.Current is { } current && chosenSet.Contains(current.ItemID) ? [.. otherList, current] : otherList;

        return await RunAll([.. removeList.Select(x => (x.ItemID, (Func<Task>)(() => serviceControl.RemoveFromQueue(x.ItemID))))]);
    }

    // Last first: each one goes to the front, so the one handled last is the first in the queue. A move takes an item of the source part into the user queue under its own ID, so nothing is queued twice.
    public async Task<BulkResult> PlayNext(List<Guid> itemIDList)
    {
        HashSet<Guid> chosenSet = [.. itemIDList];
        ResponseQueue queue = serviceQueue.GetSnapshot();
        List<QueueItem> upcomingList = [.. queue.UserQueueList.Concat(queue.SourceList).Where(x => chosenSet.Contains(x.ItemID))];

        return await RunAll([.. Enumerable.Reverse(upcomingList).Select(x => (x.ItemID, (Func<Task>)(() => serviceControl.MoveInQueue(x.ItemID, null))))]);
    }

    // Every step runs; one that throws is a failure of its item, and the message of the first failure is kept.
    private static async Task<BulkResult> RunAll(List<(Guid ItemID, Func<Task> Action)> stepList)
    {
        int succeededCount = 0;
        List<Guid> failedList = [];
        string? message = null;

        foreach ((Guid itemID, Func<Task> action) in stepList)
        {
            try
            {
                await action();
                succeededCount++;
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                failedList.Add(itemID);
                message ??= exception.ToActionMessage();
            }
        }

        return new(succeededCount, failedList, message);
    }
}
