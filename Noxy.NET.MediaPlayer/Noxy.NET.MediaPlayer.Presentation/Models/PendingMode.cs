namespace Noxy.NET.MediaPlayer.Presentation.Models;

public sealed class PendingMode
{
    // True from the change the user made until the server has answered the last one. While it is true the screen shows the user's mode, not the stored one.
    public bool IsPending { get; set; }

    // The wait before the newest change is sent; a later change cancels it.
    public CancellationTokenSource? Source { get; set; }
}
