using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeDelay : IDelay
{
    private readonly Lock _lock = new();

    public List<TimeSpan> DelayList { get; } = [];

    public Task Wait(TimeSpan time, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_lock) DelayList.Add(time);

        return Task.CompletedTask;
    }
}
