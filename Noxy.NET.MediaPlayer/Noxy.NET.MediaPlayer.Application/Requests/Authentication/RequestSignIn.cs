using System.ComponentModel.DataAnnotations;
using Noxy.NET.MediaPlayer.Application.Commands.Authentication;

namespace Noxy.NET.MediaPlayer.Application.Requests.Authentication;

public class RequestSignIn
{
    [Required]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(128)]
    public string Password { get; set; } = string.Empty;

    public CommandAuthenticationSignIn ToCommand()
    {
        return new(Email, Password);
    }
}
