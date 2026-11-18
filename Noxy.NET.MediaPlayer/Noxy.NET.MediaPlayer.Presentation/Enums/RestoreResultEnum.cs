namespace Noxy.NET.MediaPlayer.Presentation.Enums;

public enum RestoreResultEnum
{
    // The stored session was refreshed: the user is signed in.
    SignedIn,

    // Nothing is stored: the user has to sign in.
    NoSession,

    // The server refused the stored session: it is cleared and the user has to sign in.
    Rejected,

    // The server could not be reached (no network, or no address yet): the stored session stays and a later try may sign in.
    Offline
}
