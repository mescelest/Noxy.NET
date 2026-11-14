namespace Noxy.NET.MediaPlayer.Application.Gates;

internal sealed class UploadGateLease(Action release) : IDisposable
{
    private int _disposed;

    public void Dispose()
    {
        // Only the first call releases, so a repeated Dispose can't free a slot someone else has taken since.
        if (Interlocked.Exchange(ref _disposed, 1) == 0) release();
    }
}
