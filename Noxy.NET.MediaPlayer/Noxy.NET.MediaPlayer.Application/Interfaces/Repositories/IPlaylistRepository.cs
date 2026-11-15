using Noxy.NET.MediaPlayer.Application.Models.Playlist;
using Noxy.NET.MediaPlayer.Domain.Entities;

namespace Noxy.NET.MediaPlayer.Application.Interfaces.Repositories;

public interface IPlaylistRepository
{
    // Adds the playlist together with any entries in its EntryList.
    void AddPlaylist(EntityPlaylist playlist);

    // Scoped to the owner: another user's playlist comes back as null. The entries are not loaded.
    Task<EntityPlaylist?> FindPlaylist(Guid id, Guid userID, CancellationToken cancellationToken = default);

    // The entries are loaded, ordered by position, each with its media file and the media file's metadata.
    Task<EntityPlaylist?> FindPlaylistWithEntries(Guid id, Guid userID, CancellationToken cancellationToken = default);

    // The playlist with its entry count and no entries. Scoped to the owner: another user's playlist comes back as null.
    Task<PlaylistSummary?> FindPlaylistSummary(Guid id, Guid userID, CancellationToken cancellationToken = default);

    // Newest first, without entries.
    Task<List<PlaylistSummary>> GetPlaylistSummaryList(Guid userID, CancellationToken cancellationToken = default);

    Task<List<PlaylistStamp>> GetPlaylistStampList(Guid userID, CancellationToken cancellationToken = default);

    // The media file ID and the media file's modified time of every entry of the owner's playlist, a repeated media file once per entry. Empty for an unknown or another user's playlist.
    Task<List<PlaylistStamp>> GetEntryMediaFileStampList(Guid playlistID, Guid userID, CancellationToken cancellationToken = default);

    Task<int> CountPlaylists(Guid userID, CancellationToken cancellationToken = default);

    // The user's playlists that hold the media file, each with all of its entries ordered by position.
    Task<List<EntityPlaylist>> GetPlaylistListByMediaFileID(Guid userID, Guid mediaFileID, CancellationToken cancellationToken = default);

    // Saves the playlist row only, never its entries.
    void UpdatePlaylist(EntityPlaylist playlist);

    // Its entries go with it.
    void RemovePlaylist(EntityPlaylist playlist);

    void AddPlaylistEntry(EntityPlaylistEntry entry);
    void UpdatePlaylistEntry(EntityPlaylistEntry entry);
    void RemovePlaylistEntry(EntityPlaylistEntry entry);
}
