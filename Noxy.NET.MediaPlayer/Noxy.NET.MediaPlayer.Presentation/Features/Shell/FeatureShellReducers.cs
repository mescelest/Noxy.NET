using Fluxor;

namespace Noxy.NET.MediaPlayer.Presentation.Features.Shell;

public static class FeatureShellReducers
{
    [ReducerMethod]
    public static FeatureShellState ReduceSidebarSet(FeatureShellState state, ActionShellSidebarSet action)
    {
        return state with { SidebarOpen = action.SidebarOpen };
    }

    [ReducerMethod]
    public static FeatureShellState ReduceSidebarRestored(FeatureShellState state, ActionShellSidebarRestored action)
    {
        return state with { SidebarOpen = action.SidebarOpen };
    }
}
