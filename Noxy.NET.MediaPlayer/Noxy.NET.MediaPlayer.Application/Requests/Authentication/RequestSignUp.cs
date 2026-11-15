using System.ComponentModel.DataAnnotations;
using Noxy.NET.MediaPlayer.Application.Commands.Authentication;

namespace Noxy.NET.MediaPlayer.Application.Requests.Authentication;

public class RequestSignUp
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(16), MaxLength(128)]
    public string Password { get; set; } = string.Empty;

    public CommandAuthenticationSignUp ToCommand()
    {
        return new(Email, Password);
    }
}
