using Noxy.NET.MediaPlayer.Application.Interfaces.Signals;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeUploadSignal : IUploadSignal
{
    private TaskCompletionSource? _waiter;

    public int NotifyCount { get; private set; }
    public int WaitCount { get; private set; }
    public TimeSpan? LastTimeout { get; private set; }

    public void Notify()
    {
        NotifyCount++;
        _waiter?.TrySetResult();
    }

    // Blocks until Notify or cancellation, like the real signal does when nothing is pending.
    public async Task WaitAsync(TimeSpan timeout, CancellationToken cancellationToken)
    {
        WaitCount++;
        LastTimeout = timeout;

        TaskCompletionSource waiter = new(TaskCreationOptions.RunContinuationsAsynchronously);
        _waiter = waiter;
        await using (cancellationToken.Register(() => waiter.TrySetCanceled(cancellationToken)))
        {
            await waiter.Task;
        }
    }
}
