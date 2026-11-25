using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeDelay : IDelay
{
    private readonly Lock _lock = new();
    private readonly List<TaskCompletionSource> _pendingList = [];

    private readonly List<TimeSpan> _delayList = [];

    // The waits so far, as a copy taken under the lock: the code under test starts its waits from other threads, and a test reading the list while one starts must not see it change under it.
    public List<TimeSpan> DelayList
    {
        get
        {
            lock (_lock) return [.. _delayList];
        }
    }

    // When set, a wait lasts until the test releases it (or its token is cancelled), so a test decides when time has passed. Otherwise a wait ends at once.
    public bool IsManual { get; set; }

    public int PendingCount
    {
        get
        {
            lock (_lock) return _pendingList.Count(x => !x.Task.IsCompleted);
        }
    }

    public Task Wait(TimeSpan time, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_lock) _delayList.Add(time);
        if (!IsManual) return Task.CompletedTask;

        TaskCompletionSource tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
        cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken));
        lock (_lock) _pendingList.Add(tcs);

        return tcs.Task;
    }

    // Ends every wait that is under way.
    public void ReleaseAll()
    {
        List<TaskCompletionSource> releaseList;
        lock (_lock)
        {
            releaseList = [.. _pendingList];
            _pendingList.Clear();
        }

        foreach (TaskCompletionSource tcs in releaseList) tcs.TrySetResult();
    }
}
