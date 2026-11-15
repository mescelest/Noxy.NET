namespace Noxy.NET.MediaPlayer.API.Configurations;

public class ConfigRateLimitDownload : ConfigRateLimit
{
    public const string SectionName = "RateLimiting:Download";

    public ConfigRateLimitDownload()
    {
        PermitLimit = 10;
    }
}
