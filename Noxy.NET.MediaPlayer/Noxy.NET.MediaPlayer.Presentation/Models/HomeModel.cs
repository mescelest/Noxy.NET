using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;

namespace Noxy.NET.MediaPlayer.Presentation.Models;

public record HomeModel(List<ResponsePlaylist> RecentList, ContinueInfo? Continue, bool HasPlaylists);
