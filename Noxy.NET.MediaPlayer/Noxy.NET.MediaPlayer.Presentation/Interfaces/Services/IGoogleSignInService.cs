namespace Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;

public interface IGoogleSignInService
{
    // Lets the user sign in with Google and returns the Google ID token, or null when the user closed the sign-in or denied access.
    Task<string?> GetIdToken(CancellationToken cancellationToken);
}
