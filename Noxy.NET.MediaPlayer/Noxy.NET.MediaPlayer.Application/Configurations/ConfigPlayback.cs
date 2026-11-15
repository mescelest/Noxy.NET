using System.ComponentModel.DataAnnotations;

namespace Noxy.NET.MediaPlayer.Application.Configurations;

public class ConfigPlayback
{
    public const string SectionName = "Playback";

    [Range(1, 10_000)]
    public int MaxQueueLength { get; set; } = 1000;

    [Range(0, 3600)]
    public int PlayedWindowSeconds { get; set; } = 10;
}
