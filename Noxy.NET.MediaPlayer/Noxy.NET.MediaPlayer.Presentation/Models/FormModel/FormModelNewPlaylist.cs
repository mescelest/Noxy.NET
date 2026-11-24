using System.ComponentModel.DataAnnotations;
using Noxy.NET.MediaPlayer.Presentation.Attributes;

namespace Noxy.NET.MediaPlayer.Presentation.Models.FormModel;

public class FormModelNewPlaylist
{
    [Required]
    [DisplayText(nameof(PresentationTexts.FieldName))]
    public string Name { get; set; } = string.Empty;

    [DisplayText(nameof(PresentationTexts.FieldAllowDuplicates))]
    public bool AllowDuplicates { get; set; } = true;

    // The chosen hue, or null for the default one.
    public int? ArtHue { get; set; }
}
