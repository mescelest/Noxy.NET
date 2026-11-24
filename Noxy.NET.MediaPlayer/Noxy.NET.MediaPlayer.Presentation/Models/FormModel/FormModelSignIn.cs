using System.ComponentModel.DataAnnotations;
using Noxy.NET.MediaPlayer.Presentation.Attributes;

namespace Noxy.NET.MediaPlayer.Presentation.Models.FormModel;

public class FormModelSignIn
{
    [Required]
    [EmailAddress]
    [DisplayText(nameof(PresentationTexts.FieldEmail))]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(128)]
    [DisplayText(nameof(PresentationTexts.FieldPassword))]
    public string Password { get; set; } = string.Empty;
}
