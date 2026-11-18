namespace Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;

// What the app does once at start-up: sign in silently, bring the local lists up to date and open the hub. Does what it can and never throws, so a missing address, no network or a rejected session cannot stop the app from starting. With a stored session and no network it keeps the local data available and tries again by itself.
public interface IAppStartupService
{
    // Raised when IsRetrying changed.
    event Action? Changed;

    // True while the retry loop waits for its next try.
    bool IsRetrying { get; }

    // True once the first try to restore the session has finished, whatever it answered: before that the app does not yet know whether to show the sign-in screen.
    bool HasRestored { get; }

    Task Start();

    // Tries to sign in at once instead of at the end of the wait. Does nothing when no retry is under way.
    Task RetryNow();

    // The rest of start-up for a user who just signed in: the owner check, the syncs and the hub. Does not restore the session again, so a failed second refresh cannot turn a fresh sign-in into an offline one.
    Task Connect();
}
