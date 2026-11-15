using System.ComponentModel.DataAnnotations;
using Noxy.NET.MediaPlayer.Application.Attributes;
using Noxy.NET.MediaPlayer.Application.Commands.Metadata;

namespace Noxy.NET.MediaPlayer.Application.Requests.Metadata;

public class RequestCreateGenre
{
    [NotBlank]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    public CommandGenreCreate ToCommand(Guid userID)
    {
        return new(userID, Name);
    }
}
