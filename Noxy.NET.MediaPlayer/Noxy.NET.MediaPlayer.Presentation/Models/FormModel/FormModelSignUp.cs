using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Noxy.NET.MediaPlayer.Presentation.Models.FormModel;

public class FormModelSignUp
{
    [Required]
    [EmailAddress]
    [DisplayName("Email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(16), MaxLength(128)]
    [DisplayName("Password")]
    public string Password { get; set; } = string.Empty;
}
