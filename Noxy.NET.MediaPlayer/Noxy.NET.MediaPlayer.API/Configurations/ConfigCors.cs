namespace Noxy.NET.MediaPlayer.API.Configurations;

public class ConfigCors
{
    public const string SectionName = "CORS";

    public string[] Origins { get; set; } = [];
}
