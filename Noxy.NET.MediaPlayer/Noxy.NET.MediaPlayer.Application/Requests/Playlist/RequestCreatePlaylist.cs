using System.ComponentModel.DataAnnotations;
using Noxy.NET.MediaPlayer.Application.Attributes;
using Noxy.NET.MediaPlayer.Application.Commands.Playlist;

namespace Noxy.NET.MediaPlayer.Application.Requests.Playlist;

public class RequestCreatePlaylist
{
    [NotBlank]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    public bool AllowDuplicates { get; set; } = true;

    public CommandPlaylistCreate ToCommand(Guid userID)
    {
        return new(userID, Name, AllowDuplicates);
    }
}
