using Noxy.NET.MediaPlayer.Domain.Responses.Remote;
using Noxy.NET.MediaPlayer.Presentation.Enums;
using Noxy.NET.MediaPlayer.Presentation.Models;

namespace Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;

// What this device knows of the live session: the devices, the active device's state, and this device's role. It keeps the local copy of the queue in step with the server's deltas and snapshots.
public interface IRemotePlaybackService
{
    // Raised when the role, the devices or the remote state changed.
    event Action? Changed;

    // Raised each time this device finished registering with the server (after the connection was made), once the device list and the live session are known.
    event Action? Registered;

    // A transfer this device asked for did not work.
    event Action? TransferFailed;

    // Derived, never stored: Local when not connected or nobody is active, Active when this device is, Controller when another one is.
    PlaybackRoleEnum Role { get; }

    List<ResponseDevice> DeviceList { get; }

    // The active device's last reported state; null when there is none.
    RemoteState? Remote { get; }

    Guid? ActiveDeviceID { get; }

    bool IsOnline { get; }

    // A live session exists on the server (also when its active device dropped).
    bool HasSession { get; }

    // The reported position plus the time since it arrived while playing; never goes backwards between reports.
    TimeSpan GetPosition();

    // Sends this device's ID and name and stores the device list. Done by itself on every connection.
    Task Register();

    // The reporter calls this when the server accepted this device's claim, so the role is Active before the server's push arrives.
    void MarkActive();
}
