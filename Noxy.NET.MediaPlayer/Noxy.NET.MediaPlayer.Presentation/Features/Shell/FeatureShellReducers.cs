using Fluxor;

namespace Noxy.NET.MediaPlayer.Presentation.Features.Shell;

public static class FeatureShellReducers
{
    [ReducerMethod]
    public static FeatureShellState ReduceSidebarSet(FeatureShellState _, ActionShellSidebarSet action)
    {
        return new() { SidebarOpen = action.SidebarOpen };
    }

    [ReducerMethod]
    public static FeatureShellState ReduceSidebarRestored(FeatureShellState _, ActionShellSidebarRestored action)
    {
        return new() { SidebarOpen = action.SidebarOpen };
    }
}
