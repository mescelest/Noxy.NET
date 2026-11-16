using Noxy.NET.MediaPlayer.Domain.Abstractions;
using Noxy.NET.MediaPlayer.Domain.Enums;

namespace Noxy.NET.MediaPlayer.Domain.Responses.Remote;

public class ResponseQueueSource : BaseResponse
{
    public required QueueSourceKindEnum Kind { get; init; }
    public required Guid? PlaylistID { get; init; }
    public required LibrarySortEnum Sort { get; init; }
    public required bool Descending { get; init; }
    public required string Search { get; init; }
}
