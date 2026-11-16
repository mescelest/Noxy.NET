using Noxy.NET.MediaPlayer.Presentation;
using Noxy.NET.MediaPlayer.Presentation.Extensions;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;

namespace Noxy.NET.MediaPlayer.App.Stores;

public class PreferencesServerAddressStorage : IServerAddressStorage
{
    public Task<string?> GetAddress()
    {
        string address = Preferences.Default.Get(Constants.ServerAddressKey, string.Empty);

        return Task.FromResult(string.IsNullOrEmpty(address) ? null : address);
    }

    public Task SetAddress(string address)
    {
        Preferences.Default.Set(Constants.ServerAddressKey, address.NormalizeServerAddress());

        return Task.CompletedTask;
    }

    public Task ClearAddress()
    {
        Preferences.Default.Remove(Constants.ServerAddressKey);

        return Task.CompletedTask;
    }
}
