using System.ComponentModel.DataAnnotations;

namespace Noxy.NET.MediaPlayer.Domain.Requests.Playlist;

public class RequestCreatePlaylist
{
    [NotBlank]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    public bool AllowDuplicates { get; set; } = true;
}
