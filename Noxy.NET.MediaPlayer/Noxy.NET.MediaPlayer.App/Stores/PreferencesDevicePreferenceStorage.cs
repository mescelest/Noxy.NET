using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;

namespace Noxy.NET.MediaPlayer.App.Stores;

public class PreferencesDevicePreferenceStorage : IDevicePreferenceStorage
{
    public Task<string?> Get(string key)
    {
        string value = Preferences.Default.Get(key, string.Empty);

        return Task.FromResult(string.IsNullOrEmpty(value) ? null : value);
    }

    public Task Set(string key, string value)
    {
        Preferences.Default.Set(key, value);

        return Task.CompletedTask;
    }

    public Task Remove(string key)
    {
        Preferences.Default.Remove(key);

        return Task.CompletedTask;
    }
}
