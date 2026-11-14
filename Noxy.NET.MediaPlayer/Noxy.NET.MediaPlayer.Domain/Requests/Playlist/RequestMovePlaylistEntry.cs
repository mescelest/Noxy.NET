using System.ComponentModel.DataAnnotations;

namespace Noxy.NET.MediaPlayer.Domain.Requests.Playlist;

public class RequestMovePlaylistEntry
{
    [Range(0, int.MaxValue)]
    public int Position { get; set; }
}
