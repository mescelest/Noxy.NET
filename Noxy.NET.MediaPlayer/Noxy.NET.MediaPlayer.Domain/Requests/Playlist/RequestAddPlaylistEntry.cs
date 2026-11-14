using System.ComponentModel.DataAnnotations;

namespace Noxy.NET.MediaPlayer.Domain.Requests.Playlist;

public class RequestAddPlaylistEntry
{
    [Required]
    [MinLength(1)]
    [MaxLength(1000)]
    public List<Guid> SongIDList { get; set; } = [];

    // Where the first song goes. Left out, the songs are added at the end.
    [Range(0, int.MaxValue)]
    public int? Position { get; set; }
}
