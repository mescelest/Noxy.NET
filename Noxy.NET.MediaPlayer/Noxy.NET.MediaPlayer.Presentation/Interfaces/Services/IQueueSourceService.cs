using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Models;
using Noxy.NET.MediaPlayer.Domain.Responses.Remote;
using Noxy.NET.MediaPlayer.Presentation.Models;

namespace Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;

// Builds a queue source from this device's synced library and playlists. A Library item has a new ItemID each time; a playlist item's ItemID is its entry ID. Every method returns null when the source cannot be built (a playlist that is not in the local list or whose detail is not held).
public interface IQueueSourceService
{
    // The source in its own order: for the Library the search, then the sort (ties by ID); for a playlist the entries by position.
    Task<List<QueueItem>?> BuildBase(ResponseQueueSource source);

    // The source in the play order for the shuffle mode, the start first. startID is a media file ID for the Library and an entry ID for a playlist; null starts at the beginning. Also null when the start is not in the source.
    Task<List<QueueItem>?> Build(ResponseQueueSource source, ShuffleModeEnum shuffle, Guid? startID);

    // The source part after the current item (shuffle off: the source's order after it; on: the rest, shuffled), never holding the current item or an excluded one. An item is the same item when its ItemID matches for a playlist and when its MediaFileID matches for the Library.
    Task<List<QueueItem>?> Regenerate(ResponseQueueSource source, ShuffleModeEnum shuffle, QueueItem current, List<QueueItem> excludeItemList);

    // entryID names the playlist entry that is new, when the caller knows it; otherwise the first entry of that media file that is not in the queue is taken.
    // Where a media file that joined the source belongs in the queue, or null when it does not belong (it sorts before the current item, or nothing is playing).
    Task<QueuePlacement?> PlaceNew(ResponseQueueSource source, ShuffleModeEnum shuffle, LiveQueue queue, Guid mediaFileID, Guid? entryID = null);
}
