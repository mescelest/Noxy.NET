namespace Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;

// Keeps what this device holds of an account (the library, playlists, settings, files and queue) from ever being shown to another account that signs in here.
public interface ILocalDataOwnerService
{
    // Reads the signed-in user from the access token. With no owner stored it records the user and keeps the data; for the same user it does nothing; for a different user it wipes everything of the old account first and then records the new one. Signed out, or a token it cannot read, does nothing: a failure to read must never destroy data.
    Task EnsureOwner();

    // Raised after a different account signed in and the old one's data was wiped, so the screens read the empty lists at once.
    event Action? Wiped;

    // True once the owner of the signed-in account was checked, so the screens may show the local data: before that it can still be the previous account's. False again when the session ends.
    bool IsVerified { get; }

    // Raised when IsVerified changed.
    event Action? Changed;
}
