using System.ComponentModel.DataAnnotations;
using Noxy.NET.MediaPlayer.Application.Commands.Authentication;

namespace Noxy.NET.MediaPlayer.Application.Requests.Authentication;

public class RequestSignInGoogle
{
    // The ID token (a JWT) the client got from Google. Typically 1-2 KB.
    [Required]
    [MaxLength(4096)]
    public string IdToken { get; set; } = string.Empty;

    public CommandAuthenticationSignInGoogle ToCommand()
    {
        return new(IdToken);
    }
}
