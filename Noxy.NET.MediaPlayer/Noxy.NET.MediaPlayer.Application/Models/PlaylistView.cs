using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;

namespace Noxy.NET.MediaPlayer.Application.Models;

// The detail is null when the caller already has this exact playlist, so there is nothing to send.
public record PlaylistView(string ETag, ResponsePlaylistDetail? Detail);
