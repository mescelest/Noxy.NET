namespace Noxy.NET.MediaPlayer.Application.Configurations;

public class ConfigGoogle
{
    public const string SectionName = "Google";

    // The OAuth client ID from Google Cloud Console. Public, not a secret. Empty means Google login is switched off.
    public string ClientId { get; set; } = string.Empty;
}
