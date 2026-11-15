using System.ComponentModel.DataAnnotations;
using Noxy.NET.MediaPlayer.Application.Attributes;
using Noxy.NET.MediaPlayer.Application.Commands.Metadata;

namespace Noxy.NET.MediaPlayer.Application.Requests.Metadata;

public class RequestCreateAlbum
{
    public const int MaxArtistCount = 100;

    [NotBlank]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(MaxArtistCount)]
    public List<Guid> ArtistIDList { get; set; } = [];

    public CommandAlbumCreate ToCommand(Guid userID)
    {
        return new(userID, Title, ArtistIDList);
    }
}
