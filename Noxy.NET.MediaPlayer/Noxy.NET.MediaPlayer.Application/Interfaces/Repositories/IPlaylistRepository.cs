using Noxy.NET.MediaPlayer.Application.Models;
using Noxy.NET.MediaPlayer.Domain.Entities;

namespace Noxy.NET.MediaPlayer.Application.Interfaces.Repositories;

public interface IPlaylistRepository
{
    // Adds the playlist together with any entries in its EntryList.
    void AddPlaylist(Playlist playlist);

    // Scoped to the owner: another user's playlist comes back as null. The entries are not loaded.
    Task<Playlist?> FindPlaylist(Guid id, Guid userID, CancellationToken cancellationToken = default);

    // The entries are loaded, ordered by position.
    Task<Playlist?> FindPlaylistWithEntries(Guid id, Guid userID, CancellationToken cancellationToken = default);

    // Newest first, without entries.
    Task<List<PlaylistSummary>> GetPlaylistSummaryList(Guid userID, CancellationToken cancellationToken = default);

    Task<List<PlaylistStamp>> GetPlaylistStampList(Guid userID, CancellationToken cancellationToken = default);

    Task<int> CountPlaylists(Guid userID, CancellationToken cancellationToken = default);

    // The user's playlists that hold the song, each with all of its entries ordered by position.
    Task<List<Playlist>> GetPlaylistListByMediaFileID(Guid userID, Guid mediaFileID, CancellationToken cancellationToken = default);

    // Saves the playlist row only, never its entries.
    void UpdatePlaylist(Playlist playlist);

    // Its entries go with it.
    void RemovePlaylist(Playlist playlist);

    void AddPlaylistEntry(PlaylistEntry entry);
    void UpdatePlaylistEntry(PlaylistEntry entry);
    void RemovePlaylistEntry(PlaylistEntry entry);
}
