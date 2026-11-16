namespace Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;

public interface ISessionService
{
    bool IsSignedIn { get; }

    // Raised when the user becomes signed in or signed out, not on a silent refresh.
    event Action? Changed;

    Task SignUp(string email, string password);

    Task SignIn(string email, string password);

    // False when the user closed the sign-in or denied access: nothing has changed then.
    Task<bool> SignInWithGoogle(CancellationToken cancellationToken = default);

    // Signs in silently from the stored refresh token. False when there is none, when the server rejected it (it is then cleared), or when the server could not be reached (it is then kept).
    Task<bool> TryRestore();

    Task SignOut();

    // Null when signed out. Refreshes first when the token expires within the margin.
    Task<string?> GetAccessToken();

    // One refresh, shared by callers that arrive at the same time. False when the server rejected it and the user was signed out; a network failure propagates.
    Task<bool> Refresh();
}
