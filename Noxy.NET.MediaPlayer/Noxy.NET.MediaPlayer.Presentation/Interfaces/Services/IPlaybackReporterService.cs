namespace Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;

// Tells the server what this device plays while it is the active device, claims the active role for a device that plays on its own, and keeps the saved position of the local copy. A controller never reports.
public interface IPlaybackReporterService
{
    // Subscribes once to the player, the queue and the live session; a second call does nothing.
    void Start();

    // Sends the player's state now, whether or not it changed. Does nothing unless this device is the active one.
    Task ReportNow();

    // Asks the server for the active role with this device's state and queue. True: accepted (the role is Active and reports follow, from revision 2). False: refused, the player is paused. Null: the server could not be reached. Used by a device that starts to play, and by the target of a transfer.
    Task<bool?> ClaimNow();
}
