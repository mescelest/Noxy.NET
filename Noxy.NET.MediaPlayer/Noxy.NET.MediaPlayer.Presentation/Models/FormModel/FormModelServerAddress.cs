using System.ComponentModel.DataAnnotations;

namespace Noxy.NET.MediaPlayer.Presentation.Models.FormModel;

public class FormModelServerAddress
{
    [Required]
    [Url]
    [Display(Name = "Server Address")]
    public string ServerAddress { get; set; } = string.Empty;
}
