using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeDelay : IDelay
{
    private readonly Lock _lock = new();
    private readonly List<TaskCompletionSource> _pendingList = [];

    public List<TimeSpan> DelayList { get; } = [];

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
        lock (_lock) DelayList.Add(time);
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
