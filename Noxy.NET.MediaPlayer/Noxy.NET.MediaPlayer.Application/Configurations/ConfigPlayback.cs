using System.ComponentModel.DataAnnotations;

namespace Noxy.NET.MediaPlayer.Application.Configurations;

public class ConfigPlayback
{
    public const string SectionName = "Playback";

    [Range(0, 3600)]
    public int PlayedWindowSeconds { get; set; } = 10;
}
