namespace Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;

public interface IDeviceStore
{
    // Null until the device made its ID.
    Task<Guid?> GetDeviceID();

    Task SetDeviceID(Guid id);

    // Null until the user (or the service) named the device.
    Task<string?> GetDeviceName();

    Task SetDeviceName(string name);
}
