namespace Noxy.NET.MediaPlayer.Domain.Requests.Playlist;

public class RequestUpdatePlaylist
{
    [OptionalNotBlank]
    [OptionalMaxLength(255)]
    public Optional<string?> Name { get; set; }

    public Optional<bool> AllowDuplicates { get; set; }
}
