using System.ComponentModel.DataAnnotations;

namespace Noxy.NET.MediaPlayer.Domain.Requests.Authentication;

public class RequestSignUp
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    // Length only, no composition rules (OWASP ASVS: min 15 recommended, must allow at least 64).
    [Required]
    [MinLength(16), MaxLength(128)]
    public string Password { get; set; } = string.Empty;
}
