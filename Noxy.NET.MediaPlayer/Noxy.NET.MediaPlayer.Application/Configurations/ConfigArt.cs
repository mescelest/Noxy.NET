using System.ComponentModel.DataAnnotations;

namespace Noxy.NET.MediaPlayer.Application.Configurations;

public class ConfigArt
{
    public const string SectionName = "Art";

    [Range(32, 2048)]
    public int ThumbnailMaxPixels { get; set; } = 300;
}
