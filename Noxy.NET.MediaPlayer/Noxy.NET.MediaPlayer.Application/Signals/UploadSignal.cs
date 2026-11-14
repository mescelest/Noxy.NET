using Noxy.NET.MediaPlayer.Application.Interfaces.Signals;

namespace Noxy.NET.MediaPlayer.Application.Signals;

public sealed class UploadSignal : IUploadSignal
{
    private readonly SemaphoreSlim _semaphore = new(0, 1);

    public void Notify()
    {
        try
        {
            _semaphore.Release();
        }
        catch (SemaphoreFullException)
        {
            // Already signalled: the waiter has not woken yet, so this one is covered.
        }
    }

    public async Task WaitAsync(TimeSpan timeout, CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(timeout, cancellationToken);
    }
}
