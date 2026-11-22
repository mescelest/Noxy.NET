using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Noxy.NET.MediaPlayer.Presentation.Models.FormModel;

public class FormModelEditPlaylist
{
    [Required]
    [MaxLength(255)]
    [DisplayName("Name")]
    public string Name { get; set; } = string.Empty;

    [DisplayName("Allow the same song twice")]
    public bool AllowDuplicates { get; set; } = true;
}
