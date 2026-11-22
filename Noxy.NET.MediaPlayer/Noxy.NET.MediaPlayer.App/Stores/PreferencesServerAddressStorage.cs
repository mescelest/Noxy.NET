using System.Text.Json;
using Noxy.NET.MediaPlayer.App.Resources;
using Noxy.NET.MediaPlayer.Presentation;
using Noxy.NET.MediaPlayer.Presentation.Extensions;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;

namespace Noxy.NET.MediaPlayer.App.Stores;

public class PreferencesServerAddressStorage : IServerAddressStorage
{
    public Task<string?> GetAddress()
    {
        return Task.FromResult(ReadCurrent());
    }

    public Task SetAddress(string address)
    {
        string normalized = address.NormalizeServerAddress();

        List<string> list = ReadList();
        if (!list.Contains(normalized))
        {
            list.Add(normalized);
            WriteList(list);
        }

        Preferences.Default.Set(PresentationConstants.ServerAddressKey, normalized);

        return Task.CompletedTask;
    }

    public Task ClearAddress()
    {
        Preferences.Default.Remove(PresentationConstants.ServerAddressKey);

        return Task.CompletedTask;
    }

    public Task<List<string>> GetAddressList()
    {
        return Task.FromResult(ReadList());
    }

    public Task ReplaceAddress(string oldAddress, string newAddress)
    {
        string normalized = newAddress.NormalizeServerAddress();

        List<string> list = ReadList();
        int index = list.IndexOf(oldAddress);
        if (index < 0) throw new InvalidOperationException(AppTexts.ServerAddressNotSaved);
        if (normalized != oldAddress && list.Contains(normalized)) throw new InvalidOperationException(AppTexts.ServerAddressAlreadySaved);

        list[index] = normalized;
        WriteList(list);

        if (ReadCurrent() == oldAddress)
        {
            Preferences.Default.Set(PresentationConstants.ServerAddressKey, normalized);
        }

        return Task.CompletedTask;
    }

    public Task RemoveAddress(string address)
    {
        List<string> list = ReadList();
        if (list.Remove(address))
        {
            WriteList(list);
        }

        if (ReadCurrent() == address)
        {
            Preferences.Default.Remove(PresentationConstants.ServerAddressKey);
        }

        return Task.CompletedTask;
    }

    private static string? ReadCurrent()
    {
        string address = Preferences.Default.Get(PresentationConstants.ServerAddressKey, string.Empty);

        return string.IsNullOrEmpty(address) ? null : address;
    }

    // The address in use is always part of the list, which also covers an address saved before the list existed.
    private static List<string> ReadList()
    {
        List<string> list;
        try
        {
            list = JsonSerializer.Deserialize<List<string>>(Preferences.Default.Get(PresentationConstants.ServerAddressListKey, string.Empty)) ?? [];
        }
        catch (JsonException)
        {
            list = [];
        }

        string? current = ReadCurrent();
        if (current is not null && !list.Contains(current))
        {
            list.Add(current);
        }

        return list;
    }

    private static void WriteList(List<string> list)
    {
        Preferences.Default.Set(PresentationConstants.ServerAddressListKey, JsonSerializer.Serialize(list));
    }
}
