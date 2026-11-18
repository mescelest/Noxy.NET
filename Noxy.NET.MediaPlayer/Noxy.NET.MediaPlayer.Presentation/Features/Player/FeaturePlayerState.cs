using Fluxor;
using Noxy.NET.MediaPlayer.Presentation.Models;

namespace Noxy.NET.MediaPlayer.Presentation.Features.Player;

[FeatureState]
public record FeaturePlayerState
{
    public PlayerSnapshot Snapshot { get; init; } = PlayerSnapshot.Disabled;
}
