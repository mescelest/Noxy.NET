namespace Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;

// Keeps the device's copy of the library and the playlists current. A failed network call never changes the local lists or ETags: it only sets IsOffline.
public interface ISyncService
{
    // Raised after the local copy changed, and when IsOffline changed.
    event Action? Changed;

    // True when the last call could not reach the server (no address saved, network down or timed out).
    bool IsOffline { get; }

    // Sends the stored ETag unless force is set or the local list is empty, so cleared app data is refilled instead of staying empty on a 304.
    Task SyncLibrary(bool force = false);

    // The same for the playlist list; a held detail whose playlist has a later TimeModified is downloaded again.
    Task SyncPlaylistList(bool force = false);

    // Always a full download. A 404 deletes the playlist locally.
    Task SyncPlaylistDetail(Guid id);
}
