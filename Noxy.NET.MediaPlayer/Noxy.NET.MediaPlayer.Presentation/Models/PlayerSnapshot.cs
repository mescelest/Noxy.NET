using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Models;

namespace Noxy.NET.MediaPlayer.Presentation.Models;

// What the player bar shows. The queue itself is read from IQueueService when the queue panel opens, so a tick of the position never copies it.
public record PlayerSnapshot(QueueItem? Current, bool IsDisabled, bool IsPlaying, bool IsWaiting, TimeSpan Position, TimeSpan Duration, double Volume, ShuffleModeEnum Shuffle, RepeatModeEnum Repeat, Guid? PlaylistID, int QueueItemCount)
{
    public static PlayerSnapshot Disabled { get; } = new(null, true, false, false, TimeSpan.Zero, TimeSpan.Zero, 1, ShuffleModeEnum.Off, RepeatModeEnum.No, null, 0);
}
