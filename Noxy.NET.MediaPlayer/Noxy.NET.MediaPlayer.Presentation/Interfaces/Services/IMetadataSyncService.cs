using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;

namespace Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;

public interface IMetadataSyncService
{
    // Raised after a local list changed.
    event Action? Changed;

    // Sends the stored ETag unless force is set or the local list is empty, so cleared app data is refilled instead of staying empty on a 304.
    // Each list has its own ETag. A failed network call never changes a list or its ETag.
    Task SyncArtistList(bool force = false);

    Task SyncAlbumList(bool force = false);

    Task SyncGenreList(bool force = false);

    // The push handlers. A push never writes an ETag, and an older TimeModified never overwrites the local copy; an equal one does, since a count of media files changes without the item row changing.
    Task ApplyArtistUpdated(ResponseArtist artist);

    // An ID that is not held is harmless.
    Task ApplyArtistDeleted(Guid id);

    Task ApplyAlbumUpdated(ResponseAlbum album);

    Task ApplyAlbumDeleted(Guid id);

    Task ApplyGenreUpdated(ResponseGenre genre);

    Task ApplyGenreDeleted(Guid id);
}
