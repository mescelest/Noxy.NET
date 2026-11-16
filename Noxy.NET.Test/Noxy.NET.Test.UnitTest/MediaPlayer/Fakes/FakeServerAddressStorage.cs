using Noxy.NET.MediaPlayer.Presentation.Extensions;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeServerAddressStorage(string? address = "https://server.example") : IServerAddressStorage
{
    public string? Address { get; set; } = address;

    public Task<string?> GetAddress()
    {
        return Task.FromResult(Address);
    }

    public Task SetAddress(string address)
    {
        Address = address.NormalizeServerAddress();
        return Task.CompletedTask;
    }

    public Task ClearAddress()
    {
        Address = null;
        return Task.CompletedTask;
    }
}
