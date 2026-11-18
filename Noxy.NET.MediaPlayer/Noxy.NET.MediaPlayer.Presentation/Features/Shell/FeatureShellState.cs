using Fluxor;

namespace Noxy.NET.MediaPlayer.Presentation.Features.Shell;

[FeatureState]
public record FeatureShellState
{
    public bool SidebarOpen { get; init; } = true;
}
