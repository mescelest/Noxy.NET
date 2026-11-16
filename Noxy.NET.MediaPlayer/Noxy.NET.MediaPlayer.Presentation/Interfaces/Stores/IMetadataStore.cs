using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;

namespace Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;

// The device's copy of the library and the playlists, with the ETags they were downloaded under. Lists keep an order: the one they were replaced with, with a saved item that is new after the others and a saved item that exists staying where it is. A Replace is one atomic write: a failure leaves the old list.
public interface IMetadataStore
{
    Task<List<ResponseMediaFile>> GetMediaFileList();

    Task ReplaceMediaFileList(List<ResponseMediaFile> mediaFileList);

    Task SaveMediaFile(ResponseMediaFile mediaFile);

    // An ID that is not held is harmless.
    Task DeleteMediaFile(Guid id);

    Task<List<ResponsePlaylist>> GetPlaylistList();

    // Also drops the held detail of every playlist that is no longer in the list.
    Task ReplacePlaylistList(List<ResponsePlaylist> playlistList);

    // Saves the summary only: a held detail stays as it is.
    Task SavePlaylist(ResponsePlaylist playlist);

    // Also removes that playlist's held detail. An ID that is not held is harmless.
    Task DeletePlaylist(Guid id);

    Task<ResponsePlaylistDetail?> GetPlaylistDetail(Guid id);

    // Saving a detail makes its playlist held, whether or not the summary list has it.
    Task SavePlaylistDetail(ResponsePlaylistDetail detail);

    // The IDs of the playlists whose detail is held.
    Task<List<Guid>> GetHeldPlaylistIDList();

    // Only ever one that arrived with a full 200 response the device downloaded. Null clears it.
    Task<string?> GetLibraryETag();

    Task SetLibraryETag(string? eTag);

    Task<string?> GetPlaylistListETag();

    Task SetPlaylistListETag(string? eTag);
}
