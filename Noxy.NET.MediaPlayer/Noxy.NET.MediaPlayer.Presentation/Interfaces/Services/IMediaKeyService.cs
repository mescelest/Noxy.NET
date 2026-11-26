using Noxy.NET.MediaPlayer.Presentation.Models;

namespace Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;

// The media keys of the keyboard (and the system media overlay) as the platform gives them, also while the app is in the background. A platform without them registers no implementation. The events arrive on any thread.
public interface IMediaKeyService
{
    event Action? PlayPausePressed;

    event Action? NextPressed;

    event Action? PreviousPressed;

    event Action? StopPressed;

    // What the overlay shows. Applied at once when the platform is ready, otherwise as soon as it is.
    void SetState(MediaKeyState state);
}
