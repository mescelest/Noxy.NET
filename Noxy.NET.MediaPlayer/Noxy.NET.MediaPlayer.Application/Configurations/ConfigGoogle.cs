namespace Noxy.NET.MediaPlayer.Application.Configurations;

public class ConfigGoogle
{
    public const string SectionName = "Google";

    // The OAuth client IDs from Google Cloud Console, one for each app that signs in (Android, Windows, web). Public, not secrets. A token is accepted when it was issued for any of them. Empty means Google login is switched off.
    public List<string> ClientIdList { get; set; } = [];

    // The list without blank entries and without surrounding spaces.
    public List<string> ConfiguredClientIdList => [.. ClientIdList.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim())];
}
