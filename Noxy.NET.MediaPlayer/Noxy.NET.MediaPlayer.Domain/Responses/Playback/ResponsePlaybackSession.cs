using Noxy.NET.MediaPlayer.Domain.Abstractions;

namespace Noxy.NET.MediaPlayer.Domain.Responses.Playback;

public class ResponsePlaybackSession : BaseResponse
{
    public required Guid ID { get; init; }
    public required List<Guid> QueueMediaFileIDList { get; init; }
    public required int? CurrentIndex { get; init; }
    public required double PositionSeconds { get; init; }
    public required Guid? PlaylistID { get; init; }
    public required DateTime TimeModified { get; init; }
}
