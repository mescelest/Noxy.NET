using Noxy.NET.MediaPlayer.Presentation.Models;

namespace Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;

// Puts media files on the device. A file counts as on the device only when its bytes are in the file store and their SHA-256 equals the contentHash of the library entry they were downloaded for. Nothing here ever throws for a failed download: the state of the media file says what happened.
public interface IDownloadService
{
    // Raised after the file state of any media file changed.
    event Action? Changed;

    // Downloads the media file if it is not already on the device with the current hash, and returns true when it is verified on the device. A second call for a media file that is downloading waits for that download. False for an unknown media file, a failed download and a cancelled one.
    Task<bool> EnsureOnDevice(Guid mediaFileID);

    // Starts the downloads and returns: it does not wait for them. At most Constants.MaxParallelDownloads run at once.
    Task Enqueue(List<Guid> mediaFileIDList);

    // Stops a running or waiting download, discards its partial file and leaves the media file NotDownloaded. Returns when it has stopped. A media file waiting in the bulk queue leaves it.
    Task Cancel(Guid mediaFileID);

    // Takes the media file off the device: stops its download, deletes the file and its state, and raises Changed. The library entry is not touched. A media file that is not on the device is harmless.
    Task Remove(Guid mediaFileID);

    // The bulk download: queues the media files in the list that are not already on the device (and not already in the bulk) and returns at once. Before each file it checks that the free space holds the file and Constants.StorageReserveBytes; if not, the bulk stops, says why in the progress, and the rest stay NotDownloaded. Only the list passed in is queued: media files added to the library later are not.
    Task QueueMissing(List<Guid> mediaFileIDList);

    // A snapshot of the running or last bulk download. All zeros when there has been none.
    DownloadProgress GetProgress();
}
