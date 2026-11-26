using System.ComponentModel.DataAnnotations;
using Noxy.NET.MediaPlayer.Domain.Declarations;
using Noxy.NET.MediaPlayer.Presentation.Attributes;

namespace Noxy.NET.MediaPlayer.Presentation.Models.FormModel;

public class FormModelMetadataItem
{
    [Required]
    [MaxLength(255)]
    [DisplayText(nameof(PresentationTexts.FieldName))]
    public string Name { get; set; } = string.Empty;

    // Where an artist or a genre stands in the list of a song; the album has none.
    [Range(MetadataWeight.Minimum, MetadataWeight.Maximum)]
    [DisplayText(nameof(PresentationTexts.FieldWeight))]
    public int? Weight { get; set; } = MetadataWeight.Default;

    // The artists of an album; the other kinds have none.
    public HashSet<Guid> ArtistIDSet { get; set; } = [];
}
