using Fluxor;

namespace Noxy.NET.MediaPlayer.Presentation.Features.Download;

public static class FeatureDownloadReducers
{
    [ReducerMethod]
    public static FeatureDownloadState ReduceChanged(FeatureDownloadState state, ActionDownloadChanged action)
    {
        return state with { Progress = action.Progress };
    }
}
