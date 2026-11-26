using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;
using Noxy.NET.MediaPlayer.Presentation.Models;

namespace Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;

// Keeps the device's copy of the library and the playlists current. A failed network call never changes the local lists or ETags: it only sets IsOffline.
public interface ISyncService
{
    // Raised after the local copy changed, and when IsOffline changed.
    event Action? Changed;

    // Raised after the library changed, when at least one media file was added or removed. Not raised for a change that only touches metadata.
    event Action<LibraryDiff>? LibraryDiffApplied;

    // Raised for every upload result the hub reports for this user, from this device or another, after the library has been updated.
    event Action<ResponseUploadResult>? UploadResultReceived;

    // True when the last call could not reach the server (no address saved, network down or timed out).
    bool IsOffline { get; }

    // Sends the stored ETag unless force is set or the local list is empty, so cleared app data is refilled instead of staying empty on a 304.
    Task SyncLibrary(bool force = false);

    // The same for the playlist list; a held detail whose playlist has a later TimeModified is downloaded again.
    Task SyncPlaylistList(bool force = false);

    // Always a full download. A 404 deletes the playlist locally.
    Task SyncPlaylistDetail(Guid id);

    // The push handlers. A push never writes an ETag.
    Task ApplyMediaUpdated(ResponseMediaFile mediaFile);

    // An ID that is not held is harmless.
    Task ApplyMediaDeleted(Guid id);

    // An older TimeModified never overwrites a newer local copy; an equal one applies only with a later TimeLastPlayed. A held detail is downloaded again only when TimeModified is later than the detail's.
    Task ApplyPlaylistUpdated(ResponsePlaylist playlist);

    Task ApplyPlaylistDeleted(Guid id);

    // The user's whole playlist list in its new order, from the answer of a move; it replaces the stored list and writes no ETag.
    Task ApplyPlaylistOrder(List<ResponsePlaylist> playlistList);

    // A failed upload carries no media: nothing changes.
    Task ApplyUploadSucceeded(ResponseUploadResult result);

    Task ApplyUploadFailed(ResponseUploadResult result);
}
