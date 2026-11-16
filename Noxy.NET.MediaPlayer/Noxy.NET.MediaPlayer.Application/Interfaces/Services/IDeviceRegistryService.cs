using Noxy.NET.MediaPlayer.Application.Requests.Remote;
using Noxy.NET.MediaPlayer.Domain.Responses.Remote;

namespace Noxy.NET.MediaPlayer.Application.Interfaces.Services;

public interface IDeviceRegistryService
{
    // Raised after a device's current connection went away: the user, then the device.
    event Func<Guid, Guid, Task>? DeviceDisconnected;

    // Tells the registry which device of an account is the active one, so a pushed list can mark it.
    Func<Guid, Guid?>? ActiveDeviceResolver { get; set; }

    Task<List<ResponseDevice>> Register(Guid userID, RequestRegisterDevice request, string connectionID);

    Task Disconnect(string connectionID);

    List<ResponseDevice> GetDeviceList(Guid userID, Guid? activeDeviceID);

    string? GetConnectionID(Guid userID, Guid deviceID);

    (Guid UserID, Guid DeviceID)? Find(string connectionID);

    bool IsOnline(Guid userID, Guid deviceID);
}
