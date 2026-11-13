using System.ComponentModel.DataAnnotations;

namespace Noxy.NET.MediaPlayer.API.Configurations;

public class ConfigRateLimitAuth
{
    public const string SectionName = "RateLimiting:Auth";

    [Range(1, 10_000)]
    public int PermitLimit { get; set; } = 10;

    [Range(1, 86_400)]
    public int WindowSeconds { get; set; } = 60;

    public TimeSpan Window => TimeSpan.FromSeconds(WindowSeconds);
}
