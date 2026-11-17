namespace Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;

// What this device does when the server asks it to: while it is the active device it carries out the commands a controller sends, it stops when a transfer takes playback away, and it starts when a transfer hands playback to it. A controller adopts the volume of the active device.
public interface IRemoteCommandHandlerService
{
    // Subscribes once to the hub and the live session; a second call does nothing.
    void Start();
}
