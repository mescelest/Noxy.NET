using System.ComponentModel.DataAnnotations;
using Noxy.NET.MediaPlayer.Application.Attributes;
using Noxy.NET.MediaPlayer.Application.Commands.Metadata;

namespace Noxy.NET.MediaPlayer.Application.Requests.Metadata;

public class RequestUpdateArtist
{
    [NotBlank]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    public CommandArtistRename ToCommand(Guid userID, Guid id)
    {
        return new(userID, id, Name);
    }
}
