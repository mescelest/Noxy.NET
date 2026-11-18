using System.ComponentModel.DataAnnotations;

namespace Noxy.NET.MediaPlayer.Presentation.Models;

// What the create-account tab collects; the limits are the server's (RequestSignUp).
public class ModelSignUp
{
    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(16), MaxLength(128)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;
}
