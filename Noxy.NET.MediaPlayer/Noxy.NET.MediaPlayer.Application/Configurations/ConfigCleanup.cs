using System.ComponentModel.DataAnnotations;

namespace Noxy.NET.MediaPlayer.Application.Configurations;

public class ConfigCleanup
{
    public const string SectionName = "Cleanup";

    [Range(1, 720)]
    public int IntervalHours { get; set; } = 24;

    // How long an expired token is kept before it is deleted.
    [Range(0, 365)]
    public int RetentionDays { get; set; } = 7;
}
