using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeLocalDataOwnerService : ILocalDataOwnerService
{
    public event Action? Wiped;

    public event Action? Changed;

    // What IsVerified answers. A test that does not care leaves it true.
    public bool IsVerified { get; set; } = true;

    public int EnsureOwnerCount { get; private set; }

    // Runs inside EnsureOwner, so a test can record what had happened by then.
    public Action? OnEnsureOwner { get; set; }

    public void RaiseWiped()
    {
        Wiped?.Invoke();
    }

    public void RaiseChanged()
    {
        Changed?.Invoke();
    }

    public int WipeLocalDataCount { get; private set; }

    // Runs inside WipeLocalData, so a test can record what had happened by then.
    public Action? OnWipeLocalData { get; set; }

    public Task WipeLocalData()
    {
        WipeLocalDataCount++;
        OnWipeLocalData?.Invoke();

        return Task.CompletedTask;
    }

    public Task EnsureOwner()
    {
        EnsureOwnerCount++;
        OnEnsureOwner?.Invoke();

        return Task.CompletedTask;
    }
}
