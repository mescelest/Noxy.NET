namespace Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;

// Carries what the services hold into the Fluxor state the screens read. Scoped: it uses the dispatcher of the scope the screens run in. A service raises Changed; the bridge reads what changed and dispatches it.
public interface IStateBridgeService
{
    // Subscribes once to the services and loads the current state; a second call does nothing.
    void Start();

    // Reads everything again and dispatches it.
    Task Refresh();
}
