using System.ComponentModel.DataAnnotations;
using Noxy.NET.MediaPlayer.Application.Commands.Authentication;

namespace Noxy.NET.MediaPlayer.Application.Requests.Authentication;

public class RequestRefresh
{
    [Required]
    [MaxLength(128)]
    public string RefreshToken { get; set; } = string.Empty;

    public CommandAuthenticationRefresh ToCommand()
    {
        return new(RefreshToken);
    }
}
