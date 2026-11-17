using Noxy.NET.MediaPlayer.Domain.Responses.Remote;
using Noxy.NET.MediaPlayer.Presentation.Enums;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;
using Noxy.NET.MediaPlayer.Presentation.Models;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeRemotePlaybackService : IRemotePlaybackService
{
    public event Action? Changed;

    public event Action? Registered;

    public event Action? TransferFailed;

    public PlaybackRoleEnum Role { get; set; } = PlaybackRoleEnum.Local;

    public List<ResponseDevice> DeviceList { get; set; } = [];

    public RemoteState? Remote { get; set; }

    public Guid? ActiveDeviceID { get; set; }

    public bool IsOnline { get; set; } = true;

    public bool HasSession { get; set; }

    public TimeSpan Position { get; set; }

    public int RegisterCount { get; private set; }

    public int MarkActiveCount { get; private set; }

    public TimeSpan GetPosition()
    {
        return Position;
    }

    public Task Register()
    {
        RegisterCount++;
        return Task.CompletedTask;
    }

    public void MarkActive()
    {
        MarkActiveCount++;
        Role = PlaybackRoleEnum.Active;
        HasSession = true;
        Changed?.Invoke();
    }

    public void RaiseChanged()
    {
        Changed?.Invoke();
    }

    public void RaiseRegistered()
    {
        Registered?.Invoke();
    }

    public void RaiseTransferFailed()
    {
        TransferFailed?.Invoke();
    }
}
