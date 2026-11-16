using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeMediaHubService : IMediaHubService
{
    // Runs inside Start, so a test can record what had happened by then.
    public Action? OnStart { get; set; }

    public int StartCount { get; private set; }

    public bool IsConnected => StartCount > 0;

    public Task Start()
    {
        StartCount++;
        OnStart?.Invoke();

        return Task.CompletedTask;
    }

    public Task Stop()
    {
        return Task.CompletedTask;
    }
}
