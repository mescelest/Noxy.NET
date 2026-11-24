using System.ComponentModel.DataAnnotations;
using Noxy.NET.MediaPlayer.Application.Attributes;
using Noxy.NET.MediaPlayer.Application.Commands.Playlist;
using Noxy.NET.MediaPlayer.Domain.Declarations;

namespace Noxy.NET.MediaPlayer.Application.Requests.Playlist;

public class RequestCreatePlaylist
{
    [NotBlank]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    public bool AllowDuplicates { get; set; } = true;

    // The hue of the tile, 0 to 359; null for the one made from the ID.
    [Range(PlaylistArtHue.Min, PlaylistArtHue.Max)]
    public int? ArtHue { get; set; }

    public CommandPlaylistCreate ToCommand(Guid userID)
    {
        return new(userID, Name, AllowDuplicates, ArtHue);
    }
}
