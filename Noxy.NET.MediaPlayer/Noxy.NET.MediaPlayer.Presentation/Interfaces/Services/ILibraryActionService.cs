using Noxy.NET.MediaPlayer.Presentation.Enums;
using Noxy.NET.MediaPlayer.Presentation.Models;

namespace Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;

// The actions of the Library page on a list of media files. None of them throws for a failed call: the result says what happened.
public interface ILibraryActionService
{
    // Queues the ones that are not on the device (the low-space stop applies) and returns at once.
    Task<BulkResult> Download(List<Guid> mediaFileIDList);

    // Puts them next in the queue, in the order of the list (the first one plays first). Needs a queue: ignored while none is loaded.
    Task<BulkResult> PlayNext(List<Guid> mediaFileIDList);

    // Puts them at the end of the user queue, in the order of the list.
    Task<BulkResult> AddToQueue(List<Guid> mediaFileIDList);

    // Asks where to save each one and writes the tagged copy the server builds. A cancelled dialog is not a failure and ends the rest. Needs the server.
    Task<BulkResult> SaveAsFile(List<Guid> mediaFileIDList);

    // Takes the files off this device; the library is not touched. A playing one stops first so its file is released.
    Task<BulkResult> RemoveFromDevice(List<Guid> mediaFileIDList);

    // Deletes them on the server one by one; the push removes the rows. Needs the server.
    Task<BulkResult> DeleteFromLibrary(List<Guid> mediaFileIDList);

    // One request with all of them, at the end of the playlist. Needs the server.
    Task<BulkResult> AddToPlaylist(Guid playlistID, List<Guid> mediaFileIDList);

    // Why the action cannot be used right now, or null when it can.
    string? GetDisabledReason(LibraryActionEnum action);
}
