namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeTimeProvider : TimeProvider
{
    private readonly List<FakeTimer> _timerList = [];

    public DateTimeOffset Now { get; set; } = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

    public int ActiveTimerCount
    {
        get
        {
            lock (_timerList) return _timerList.Count(x => x.IsActive);
        }
    }

    public override DateTimeOffset GetUtcNow()
    {
        return Now;
    }

    public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
    {
        FakeTimer timer = new(this, callback, state, Now + dueTime);
        if (dueTime == Timeout.InfiniteTimeSpan) timer.Change(Timeout.InfiniteTimeSpan, period);

        lock (_timerList) _timerList.Add(timer);
        return timer;
    }

    // Moves the clock forward and fires every timer that falls due on the way, earliest first, each at its own time.
    public void Advance(TimeSpan span)
    {
        DateTimeOffset target = Now + span;
        while (true)
        {
            FakeTimer? next;
            lock (_timerList) next = _timerList.Where(x => x.IsActive && x.Due <= target).OrderBy(x => x.Due).FirstOrDefault();

            if (next is null) break;

            if (next.Due > Now) Now = next.Due;

            next.Fire();
        }

        Now = target;
    }

    public void Remove(FakeTimer timer)
    {
        lock (_timerList) _timerList.Remove(timer);
    }
}
