using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;

namespace Noxy.NET.MediaPlayer.Presentation.Services;

public class DeviceService(IDeviceStore storeDevice, Func<string>? machineName = null) : IDeviceService
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
        return await storeDevice.GetDeviceName() ?? DefaultName();
    }

    // The machine's name as a device name: trimmed and cut to the length a name may have, so the server accepts it. A machine with no name is "This device".
    private string DefaultName()
    {
        string name = (machineName?.Invoke() ?? Environment.MachineName).Trim();
        if (name.Length == 0) return PresentationTexts.DefaultDeviceName;

        return name.Length > PresentationConstants.DeviceNameMaxLength ? name[..PresentationConstants.DeviceNameMaxLength].TrimEnd() : name;
    }

    public async Task SetName(string name)
    {
        string trimmed = name.Trim();
        switch (trimmed.Length)
        {
            case 0:
                throw new ArgumentException("The device name cannot be empty.", nameof(name));
            case > PresentationConstants.DeviceNameMaxLength:
                throw new ArgumentException($"The device name cannot be longer than {PresentationConstants.DeviceNameMaxLength} characters.", nameof(name));
            default:
                await storeDevice.SetDeviceName(trimmed);
                break;
        }
    }
}
