using Noxy.NET.MediaPlayer.Presentation.Models;

namespace Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;

// What the queue page does with several queue items at once. The IDs are queue item IDs. Only the items that have not played yet (the user queue and the source part) are touched: history and the item that is playing are skipped, and that is not a failure.
public interface IQueueActionService
{
    // Takes the items out of the queue. A call that fails is a failure of that item and the others still go.
    Task<BulkResult> Remove(List<Guid> itemIDList);

    // Puts the items at the front of the user queue, in the order they have in the queue now. An item of the source part is moved into the user queue under its own ID, so a song is never queued twice.
    Task<BulkResult> PlayNext(List<Guid> itemIDList);
}
