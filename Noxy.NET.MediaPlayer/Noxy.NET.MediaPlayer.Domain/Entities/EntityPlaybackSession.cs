using Noxy.NET.MediaPlayer.Domain.Abstractions.Entities;

namespace Noxy.NET.MediaPlayer.Domain.Entities;

public class EntityPlaybackSession : BaseEntity
{
    public required Guid UserID { get; set; }

    // The media files in play order. The same media file may appear more than once.
    public List<Guid> QueueMediaFileIDList { get; set; } = [];

    // The place in the queue that is playing, or null when nothing is.
    public int? CurrentIndex { get; set; }

    public double PositionSeconds { get; set; }

    // The playlist the queue came from, if any. It is a hint only: a playlist that was deleted since leaves it pointing at nothing.
    public Guid? PlaylistID { get; set; }

    public DateTime TimeModified { get; set; } = DateTime.UtcNow;
}
