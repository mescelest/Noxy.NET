using Mediator;
using Microsoft.Extensions.Options;
using Noxy.NET.MediaPlayer.Application.Configurations;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Application.Models.Remote;
using Noxy.NET.MediaPlayer.Application.Notifications.Remote;
using Noxy.NET.MediaPlayer.Application.Requests.Remote;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Responses.Remote;

namespace Noxy.NET.MediaPlayer.Application.Services;

public class DeviceRegistryService(IPublisher publisher, IOptions<ConfigRemote> configRemote) : IDeviceRegistryService
{
    private readonly Lock _lock = new();
    private readonly Dictionary<Guid, Dictionary<Guid, RegisteredDevice>> _deviceMapByUser = [];
    private readonly Dictionary<string, (Guid UserID, Guid DeviceID)> _connectionMap = [];

    public event Func<Guid, Guid, Task>? DeviceDisconnected;

    public Func<Guid, Guid?>? ActiveDeviceResolver { get; set; }

    // Adds the device or replaces its earlier connection. The list is pushed only when it changed (a new device, a new name, or a device this connection used to be).
    public async Task<List<ResponseDevice>> Register(Guid userID, RequestRegisterDevice request, string connectionID)
    {
        bool changed;
        Guid? replacedDeviceID = null;
        lock (_lock)
        {
            // A connection is one device: registering another on it takes the first one off.
            if (_connectionMap.TryGetValue(connectionID, out (Guid UserID, Guid DeviceID) previous) && previous != (userID, request.DeviceID))
            {
                RemoveDevice(previous.UserID, previous.DeviceID);
                replacedDeviceID = previous.DeviceID;
            }

            if (!_deviceMapByUser.TryGetValue(userID, out Dictionary<Guid, RegisteredDevice>? deviceMap))
            {
                deviceMap = [];
                _deviceMapByUser[userID] = deviceMap;
            }

            if (deviceMap.TryGetValue(request.DeviceID, out RegisteredDevice? existing))
            {
                _connectionMap.Remove(existing.ConnectionID);
                changed = existing.Name != request.Name;
                existing.Name = request.Name;
                existing.ConnectionID = connectionID;
            }
            else
            {
                if (deviceMap.Count >= configRemote.Value.MaxDeviceCount) throw new TooManyDevicesException();

                deviceMap[request.DeviceID] = new() { DeviceID = request.DeviceID, Name = request.Name, ConnectionID = connectionID };
                changed = true;
            }

            _connectionMap[connectionID] = (userID, request.DeviceID);
        }

        List<ResponseDevice> list = GetDeviceList(userID, ActiveDeviceResolver?.Invoke(userID));
        if (changed || replacedDeviceID is not null) await publisher.Publish(new NotificationDeviceListChanged(userID, list));
        if (replacedDeviceID is { } replaced) await RaiseDeviceDisconnected(userID, replaced);

        return list;
    }

    // Removes the device only when the connection is still its current one: a replaced connection's late disconnect changes nothing.
    public async Task Disconnect(string connectionID)
    {
        Guid userID;
        Guid deviceID;
        lock (_lock)
        {
            if (!_connectionMap.TryGetValue(connectionID, out (Guid UserID, Guid DeviceID) found)) return;

            (userID, deviceID) = found;
            RemoveDevice(userID, deviceID);
        }

        // The session must hear of the drop even when the push fails.
        try
        {
            await publisher.Publish(new NotificationDeviceListChanged(userID, GetDeviceList(userID, ActiveDeviceResolver?.Invoke(userID))));
        }
        finally
        {
            await RaiseDeviceDisconnected(userID, deviceID);
        }
    }

    public List<ResponseDevice> GetDeviceList(Guid userID, Guid? activeDeviceID)
    {
        lock (_lock)
        {
            if (!_deviceMapByUser.TryGetValue(userID, out Dictionary<Guid, RegisteredDevice>? deviceMap)) return [];

            return [.. deviceMap.Values.Select(x => new ResponseDevice { DeviceID = x.DeviceID, Name = x.Name, IsActive = x.DeviceID == activeDeviceID })];
        }
    }

    public string? GetConnectionID(Guid userID, Guid deviceID)
    {
        lock (_lock)
        {
            return _deviceMapByUser.TryGetValue(userID, out Dictionary<Guid, RegisteredDevice>? deviceMap) && deviceMap.TryGetValue(deviceID, out RegisteredDevice? device) ? device.ConnectionID : null;
        }
    }

    public (Guid UserID, Guid DeviceID)? Find(string connectionID)
    {
        lock (_lock)
        {
            return _connectionMap.TryGetValue(connectionID, out (Guid UserID, Guid DeviceID) found) ? found : null;
        }
    }

    public bool IsOnline(Guid userID, Guid deviceID)
    {
        return GetConnectionID(userID, deviceID) is not null;
    }

    // Callers hold the lock.
    private void RemoveDevice(Guid userID, Guid deviceID)
    {
        if (!_deviceMapByUser.TryGetValue(userID, out Dictionary<Guid, RegisteredDevice>? deviceMap)) return;

        if (deviceMap.Remove(deviceID, out RegisteredDevice? removed)) _connectionMap.Remove(removed.ConnectionID);
        if (deviceMap.Count == 0) _deviceMapByUser.Remove(userID);
    }

    private async Task RaiseDeviceDisconnected(Guid userID, Guid deviceID)
    {
        Func<Guid, Guid, Task>? handler = DeviceDisconnected;
        if (handler is null) return;

        foreach (Func<Guid, Guid, Task> subscriber in handler.GetInvocationList().Cast<Func<Guid, Guid, Task>>())
        {
            await subscriber(userID, deviceID);
        }
    }
}
