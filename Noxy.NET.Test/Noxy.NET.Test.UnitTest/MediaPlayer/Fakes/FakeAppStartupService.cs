using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeAppStartupService : IAppStartupService
{
    public event Action? Changed;

    public bool IsRetrying { get; set; }

    public bool HasRestored { get; set; } = true;

    public int StartCount { get; private set; }

    public int RetryNowCount { get; private set; }

    public int ConnectCount { get; private set; }

    public void RaiseChanged()
    {
        Changed?.Invoke();
    }

    public Task Start()
    {
        StartCount++;
        return Task.CompletedTask;
    }

    public Task Connect()
    {
        ConnectCount++;
        return Task.CompletedTask;
    }

    public Task RetryNow()
    {
        RetryNowCount++;
        return Task.CompletedTask;
    }
}
