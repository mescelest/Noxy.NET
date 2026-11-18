using Fluxor;

namespace Noxy.NET.MediaPlayer.Presentation.Features.Session;

[FeatureState]
public record FeatureSessionState
{
    public bool IsSignedIn { get; init; }

    // Signed in, or a session is stored and only the server could not be reached.
    public bool HasSession { get; init; }

    public bool IsOffline { get; init; }
    public bool IsRetrying { get; init; }

    // The first try to restore the session has finished: before that it is not known whether the sign-in screen is needed.
    public bool IsRestored { get; init; }
}
