using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Noxy.NET.MediaPlayer.Presentation.Models.FormModel;

public class FormModelSignIn
{
    [Required]
    [EmailAddress]
    [DisplayName("Email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(128)]
    [DisplayName("Password")]
    public string Password { get; set; } = string.Empty;
}
