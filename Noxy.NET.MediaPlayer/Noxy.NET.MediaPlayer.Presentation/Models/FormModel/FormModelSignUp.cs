using System.ComponentModel.DataAnnotations;
using Noxy.NET.MediaPlayer.Presentation.Attributes;

namespace Noxy.NET.MediaPlayer.Presentation.Models.FormModel;

public class FormModelSignUp
{
    [Required]
    [EmailAddress]
    [DisplayText(nameof(PresentationTexts.FieldEmail))]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(16), MaxLength(128)]
    [DisplayText(nameof(PresentationTexts.FieldPassword))]
    public string Password { get; set; } = string.Empty;
}
