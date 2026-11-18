using Fluxor;

namespace Noxy.NET.MediaPlayer.Presentation.Features.Player;

public static class FeaturePlayerReducers
{
    [ReducerMethod]
    public static FeaturePlayerState ReduceChanged(FeaturePlayerState state, ActionPlayerChanged action)
    {
        return state with { Snapshot = action.Snapshot };
    }
}
