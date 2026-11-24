using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Noxy.NET.MediaPlayer.Application.Models;

namespace Noxy.NET.MediaPlayer.Presentation.Models.FormModel;

public class FormModelEditPlaylist
{
    private const int HueCount = 12;
    private const int HueStep = 30;

    // The colours the tile can be given, round the colour wheel.
    public static IReadOnlyList<int> HueList { get; } = [.. Enumerable.Range(0, HueCount).Select(x => x * HueStep)];

    [Required]
    [MaxLength(255)]
    [DisplayName("Name")]
    public string Name { get; set; } = string.Empty;

    [DisplayName("Allow the same song twice")]
    public bool AllowDuplicates { get; set; } = true;

    // The chosen hue, or null for the default one.
    public int? ArtHue { get; set; }

    // What goes into the update: nothing when the choice is the one the playlist already had, the hue when another was chosen, a null to go back to the default.
    public Optional<int?> GetArtHueChange(int? original)
    {
        return ArtHue == original ? default : new(ArtHue);
    }
}
