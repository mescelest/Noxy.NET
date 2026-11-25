using System.ComponentModel.DataAnnotations;
using Noxy.NET.MediaPlayer.Presentation.Attributes;

namespace Noxy.NET.MediaPlayer.Presentation.Models.FormModel;

public class FormModelMetadataItem
{
    [Required]
    [MaxLength(255)]
    [DisplayText(nameof(PresentationTexts.FieldName))]
    public string Name { get; set; } = string.Empty;

    // The artists of an album; the other kinds have none.
    public HashSet<Guid> ArtistIDSet { get; set; } = [];
}
