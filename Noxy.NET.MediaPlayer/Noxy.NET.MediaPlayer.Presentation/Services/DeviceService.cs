using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;

namespace Noxy.NET.MediaPlayer.Presentation.Services;

public class DeviceService(IDeviceStore storeDevice) : IDeviceService
{
    private readonly SemaphoreSlim _idLock = new(1, 1);

    public async Task<Guid> GetDeviceID()
    {
        await _idLock.WaitAsync();
        try
        {
            Guid? id = await storeDevice.GetDeviceID();
            if (id is not null) return id.Value;

            Guid created = Guid.NewGuid();
            await storeDevice.SetDeviceID(created);
            return created;
        }
        finally
        {
            _idLock.Release();
        }
    }

    public async Task<string> GetName()
    {
        return await storeDevice.GetDeviceName() ?? Environment.MachineName;
    }

    public async Task SetName(string name)
    {
        string trimmed = name.Trim();
        if (trimmed.Length == 0) throw new ArgumentException("The device name cannot be empty.", nameof(name));
        if (trimmed.Length > Constants.DeviceNameMaxLength) throw new ArgumentException($"The device name cannot be longer than {Constants.DeviceNameMaxLength} characters.", nameof(name));

        await storeDevice.SetDeviceName(trimmed);
    }
}
