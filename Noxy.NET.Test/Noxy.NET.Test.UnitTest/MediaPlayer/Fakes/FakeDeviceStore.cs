using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeDeviceStore : IDeviceStore
{
    private Guid? _deviceID;
    private string? _deviceName;

    public Task<Guid?> GetDeviceID()
    {
        return Task.FromResult(_deviceID);
    }

    public Task SetDeviceID(Guid id)
    {
        _deviceID = id;
        return Task.CompletedTask;
    }

    public Task<string?> GetDeviceName()
    {
        return Task.FromResult(_deviceName);
    }

    public Task SetDeviceName(string name)
    {
        _deviceName = name;
        return Task.CompletedTask;
    }
}
