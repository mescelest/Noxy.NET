namespace Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;

// A wait that a test can replace, so a retry back-off does not make a test slow.
public interface IDelay
{
    Task Wait(TimeSpan time, CancellationToken cancellationToken);
}
