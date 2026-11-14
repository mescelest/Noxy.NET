namespace Noxy.NET.MediaPlayer.Domain.Responses.Playlist;

public class ResponsePlaylistDetail : ResponsePlaylist
{
    public required List<ResponsePlaylistEntry> EntryList { get; init; }
}
