using System.ComponentModel.DataAnnotations;

namespace Noxy.NET.MediaPlayer.Domain.Requests.Authentication;

public class RequestSignIn
{
    [Required]
    public string Email { get; set; } = string.Empty;

    // No minimum here, so tightening the sign-up rule later never locks out existing users.
    [Required]
    [MaxLength(128)]
    public string Password { get; set; } = string.Empty;
}
