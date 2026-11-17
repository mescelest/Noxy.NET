namespace Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;

// Tells the server what this device plays while it is the active device, claims the active role for a device that plays on its own, and writes the local copy of the queue when playing stops. A controller never reports.
public interface IPlaybackReporterService
{
    // Subscribes once to the player, the queue and the live session; a second call does nothing.
    void Start();

    // Writes the queue and the position of the current item to the local copy. Done by itself when playing stops; the app calls it when it leaves the foreground.
    Task SaveNow();

    // Sends the player's state now, whether or not it changed. Does nothing unless this device is the active one.
    Task ReportNow();

    // Asks the server for the active role with this device's state and queue. True: accepted (the role is Active and reports follow, from revision 2). False: refused, the player is paused. Null: the server could not be reached. Used by a device that starts to play, and by the target of a transfer.
    Task<bool?> ClaimNow();
}
