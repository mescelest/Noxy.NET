using System.ComponentModel.DataAnnotations;

namespace Noxy.NET.MediaPlayer.Domain.Requests.Authentication;

public class RequestSignInGoogle
{
    // The ID token (a JWT) the client got from Google. Typically 1-2 KB.
    [Required]
    [MaxLength(4096)]
    public string IdToken { get; set; } = string.Empty;
}
