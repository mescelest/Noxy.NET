using System.ComponentModel.DataAnnotations;

namespace Noxy.NET.MediaPlayer.Domain.Requests.Playlist;

public class RequestRemovePlaylistEntry
{
    [Required]
    [MinLength(1)]
    [MaxLength(1000)]
    public List<Guid> EntryIDList { get; set; } = [];
}
