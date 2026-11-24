using System.ComponentModel.DataAnnotations;
using Noxy.NET.MediaPlayer.Application.Models;
using Noxy.NET.MediaPlayer.Presentation.Attributes;

namespace Noxy.NET.MediaPlayer.Presentation.Models.FormModel;

public class FormModelEditPlaylist
{
    [Required]
    [MaxLength(255)]
    [DisplayText(nameof(PresentationTexts.FieldName))]
    public string Name { get; set; } = string.Empty;

    [DisplayText(nameof(PresentationTexts.FieldAllowDuplicates))]
    public bool AllowDuplicates { get; set; } = true;

    // The chosen hue, or null for the default one.
    public int? ArtHue { get; set; }

    // What goes into the update: nothing when the choice is the one the playlist already had, the hue when another was chosen, a null to go back to the default.
    public Optional<int?> GetArtHueChange(int? original)
    {
        return ArtHue == original ? default : new(ArtHue);
    }
}
