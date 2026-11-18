using System.ComponentModel.DataAnnotations;

namespace Noxy.NET.MediaPlayer.Presentation.Models;

// What the sign-in tab collects; the limits are the server's (RequestSignIn).
public class ModelSignIn
{
    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(128)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;
}
