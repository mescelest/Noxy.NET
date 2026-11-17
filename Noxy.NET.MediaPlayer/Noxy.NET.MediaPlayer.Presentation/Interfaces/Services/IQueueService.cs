using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Models;
using Noxy.NET.MediaPlayer.Domain.Responses.Remote;

namespace Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;

// This device's copy of the queue. It is kept in memory and written to the local copy only by SaveLocalCopy.
public interface IQueueService
{
    // Raised after the queue changed, once per operation.
    event Action? Changed;

    // Raised for every edit this device applied itself (a user edit, an advance, a new source, a source change underneath), never for one that arrived from the server. The reporter sends them while this device is the active one.
    event Action<QueueEdit>? EditApplied;

    // Raised when a pushed delta does not follow the local revision (or cannot be applied): the caller asks the server for a snapshot.
    event Action? GapDetected;

    // Raised, on a device that owns playback, when the item it is playing was removed from the queue by a Remove: the player skips at once.
    event Action? CurrentRemoved;

    // A copy of the queue as it is now, taken while no edit is half done. Use this, not Queue.ToResponse(), from outside the service.
    ResponseQueue GetSnapshot();

    // The current queue. A snapshot or a loaded copy replaces the instance, so read it again after Changed. Its Current, Source and counts are safe to read; to copy the lists use GetSnapshot.
    LiveQueue Queue { get; }

    // True while this device is the active one or plays alone: only then does a change of the library or a playlist reach the queue, and only then does a removed current item raise CurrentRemoved. Set by the reporter.
    bool OwnsPlayback { get; set; }

    // Applies an edit to the local queue. The local copy is not written for it: it is written by SaveLocalCopy. False when it changed nothing (an item that is gone is not an error); throws InvalidQueueEditException for an edit that is not valid.
    Task<bool> Apply(QueueEdit edit);

    // The pushed form of an edit: exactly the local revision + 1 is applied, a lower or equal one is ignored (this device applied it already), a higher one raises GapDetected.
    Task ApplyDelta(ResponseQueueDelta delta);

    // Replaces the copy. With keepLocalPointer (the active device) this device's own current item and history win; only a snapshot with no current item at all (the playing item was removed) is taken as it is, and CurrentRemoved is raised.
    Task ReplaceWithSnapshot(ResponseQueue snapshot, bool keepLocalPointer);

    // Builds the source and makes it the queue, the start item first (startID is a media file for the Library, an entry for a playlist; null starts at the beginning). The user queue is kept when the setting says so. False, changing nothing, when the source cannot be built or is empty.
    Task<bool> StartSource(ResponseQueueSource source, Guid? startID, ShuffleModeEnum shuffle);

    // Builds the source part again from the current item. Does nothing without a source or a current item.
    Task Regenerate(ShuffleModeEnum shuffle);

    // Loads the saved copy; returns its position, or null when nothing (usable) was saved.
    Task<TimeSpan?> LoadLocalCopy();

    // Writes the queue and the position of the current item to the local copy now. Changes are otherwise only kept in memory, so this is called when playback pauses or stops and when the app leaves the foreground.
    Task SaveLocalCopy(double positionSeconds);

    // Empties the queue and the local copy (an account switch).
    Task Clear();
}
