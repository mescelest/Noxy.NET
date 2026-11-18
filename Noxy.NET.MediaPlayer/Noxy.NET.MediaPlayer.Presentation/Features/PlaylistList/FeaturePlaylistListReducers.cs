using Fluxor;

namespace Noxy.NET.MediaPlayer.Presentation.Features.PlaylistList;

public static class FeaturePlaylistListReducers
{
    [ReducerMethod]
    public static FeaturePlaylistListState ReduceLoaded(FeaturePlaylistListState state, ActionPlaylistListLoaded action)
    {
        return state with { PlaylistList = action.PlaylistList };
    }
}
