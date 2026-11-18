using Fluxor;

namespace Noxy.NET.MediaPlayer.Presentation.Features.Session;

public static class FeatureSessionReducers
{
    [ReducerMethod]
    public static FeatureSessionState ReduceChanged(FeatureSessionState _, ActionSessionChanged action)
    {
        return new()
        {
            IsSignedIn = action.IsSignedIn,
            HasSession = action.HasSession,
            IsOffline = action.IsOffline,
            IsRetrying = action.IsRetrying,
            IsRestored = action.IsRestored
        };
    }
}
