namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeTimer(FakeTimeProvider provider, TimerCallback callback, object? state, DateTimeOffset due) : ITimer
{
    public DateTimeOffset Due { get; private set; } = due;
    public bool IsActive { get; private set; } = true;

    public bool Change(TimeSpan dueTime, TimeSpan period)
    {
        Due = provider.Now + dueTime;
        IsActive = dueTime != Timeout.InfiniteTimeSpan;
        return true;
    }

    public void Fire()
    {
        IsActive = false;
        callback(state);
    }

    public void Dispose()
    {
        IsActive = false;
        provider.Remove(this);
    }

    public ValueTask DisposeAsync()
    {
        Dispose();
        return ValueTask.CompletedTask;
    }
}
