namespace Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;

// Small choices that belong to this device and not to the account (the sidebar, the theme). The App keeps them in the preferences of the platform.
public interface IDevicePreferenceStorage
{
    // Null when nothing is stored under the key.
    Task<string?> Get(string key);

    Task Set(string key, string value);

    Task Remove(string key);
}
