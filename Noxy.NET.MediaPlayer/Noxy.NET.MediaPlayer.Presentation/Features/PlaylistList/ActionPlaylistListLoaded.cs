using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;

namespace Noxy.NET.MediaPlayer.Presentation.Features.PlaylistList;

public record ActionPlaylistListLoaded(List<ResponsePlaylist> PlaylistList);
