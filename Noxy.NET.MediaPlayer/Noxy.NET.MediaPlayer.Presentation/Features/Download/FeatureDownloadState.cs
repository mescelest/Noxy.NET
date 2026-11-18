using Fluxor;
using Noxy.NET.MediaPlayer.Presentation.Models;

namespace Noxy.NET.MediaPlayer.Presentation.Features.Download;

[FeatureState]
public record FeatureDownloadState
{
    public DownloadProgress Progress { get; init; } = new(0, 0, 0, 0, null);
}
