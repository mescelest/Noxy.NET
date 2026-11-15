using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;

namespace Noxy.NET.MediaPlayer.Application.Models.Playlist;

// The list is null when the caller already has these exact playlists, so there is nothing to send.
public record PlaylistLibrary(string ETag, List<ResponsePlaylist>? PlaylistList);
