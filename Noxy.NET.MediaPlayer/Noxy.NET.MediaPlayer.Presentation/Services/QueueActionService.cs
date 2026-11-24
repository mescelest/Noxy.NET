using Noxy.NET.MediaPlayer.Domain.Models;
using Noxy.NET.MediaPlayer.Domain.Responses.Remote;
using Noxy.NET.MediaPlayer.Presentation.Extensions;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;
using Noxy.NET.MediaPlayer.Presentation.Models;

namespace Noxy.NET.MediaPlayer.Presentation.Services;

public class QueueActionService(IPlaybackControlService serviceControl, IQueueService serviceQueue) : IQueueActionService
{
    public async Task<BulkResult> Remove(List<Guid> itemIDList)
    {
        HashSet<Guid> chosenSet = [.. itemIDList];
        ResponseQueue queue = serviceQueue.GetSnapshot();
        List<QueueItem> upcomingList = [.. queue.UserQueueList.Concat(queue.SourceList).Where(x => chosenSet.Contains(x.ItemID))];

        return await RunAll([.. upcomingList.Select(x => (x.ItemID, (Func<Task>)(() => serviceControl.RemoveFromQueue(x.ItemID))))]);
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
