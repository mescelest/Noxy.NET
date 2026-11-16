namespace Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;

// The live connection to the server's hub. Pushes are forwarded to the sync service; after a reconnect both lists are synced again, since pushes sent while offline are lost.
public interface IMediaHubService
{
    bool IsConnected { get; }

    // Does nothing without a saved server address or a signed-in user, and never throws when the server cannot be reached.
    Task Start();

    Task Stop();
}
