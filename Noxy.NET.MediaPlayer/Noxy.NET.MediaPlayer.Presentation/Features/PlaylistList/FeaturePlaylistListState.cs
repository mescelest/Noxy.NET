using Fluxor;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;

namespace Noxy.NET.MediaPlayer.Presentation.Features.PlaylistList;

[FeatureState]
public record FeaturePlaylistListState
{
    public List<ResponsePlaylist> PlaylistList { get; init; } = [];
}
