namespace Noxy.NET.MediaPlayer.Application.Interfaces.Signals;

public interface IUploadSignal
{
    // Wakes the upload worker. Several calls before it wakes count as one.
    void Notify();

    // Returns when notified or when the timeout passes, and throws OperationCanceledException when cancelled.
    Task WaitAsync(TimeSpan timeout, CancellationToken cancellationToken);
}
