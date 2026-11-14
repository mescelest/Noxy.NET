namespace Noxy.NET.MediaPlayer.API.Configurations;

public class ConfigRateLimitUpload : ConfigRateLimit
{
    public const string SectionName = "RateLimiting:Upload";

    public ConfigRateLimitUpload()
    {
        PermitLimit = 30;
    }
}
