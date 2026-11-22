using Noxy.NET.MediaPlayer.Presentation.Extensions;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeServerAddressStorage(string? address = "https://server.example") : IServerAddressStorage
{
    private readonly List<string> _addressList = address is null ? [] : [address];

    public string? Address { get; set; } = address;

    public Task<string?> GetAddress()
    {
        return Task.FromResult(Address);
    }

    public Task SetAddress(string address)
    {
        string normalized = address.NormalizeServerAddress();
        if (!_addressList.Contains(normalized))
        {
            _addressList.Add(normalized);
        }

        Address = normalized;
        return Task.CompletedTask;
    }

    public Task ClearAddress()
    {
        Address = null;
        return Task.CompletedTask;
    }

    public Task<List<string>> GetAddressList()
    {
        return Task.FromResult(new List<string>(_addressList));
    }

    public Task ReplaceAddress(string oldAddress, string newAddress)
    {
        string normalized = newAddress.NormalizeServerAddress();
        int index = _addressList.IndexOf(oldAddress);
        if (index < 0) throw new InvalidOperationException("The server address is not saved.");
        if (normalized != oldAddress && _addressList.Contains(normalized)) throw new InvalidOperationException("This server is already saved.");

        _addressList[index] = normalized;
        if (Address == oldAddress)
        {
            Address = normalized;
        }

        return Task.CompletedTask;
    }

    public Task RemoveAddress(string address)
    {
        _addressList.Remove(address);
        if (Address == address)
        {
            Address = null;
        }

        return Task.CompletedTask;
    }
}
