using System.ComponentModel.DataAnnotations;

namespace Noxy.NET.MediaPlayer.API.Configurations;

public class ConfigConnectionString
{
    public const string SectionName = "ConnectionStrings";

    [Required]
    public string Database { get; set; } = string.Empty;
}
