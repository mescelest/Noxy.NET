using Noxy.NET.MediaPlayer.Domain.Abstractions;

namespace Noxy.NET.MediaPlayer.Domain.Responses.Playlist;

public class ResponsePlaylist : BaseResponse
{
    public required Guid ID { get; init; }
    public required string Name { get; init; }
    public required bool AllowDuplicates { get; init; }
    public required int EntryCount { get; init; }
    public required DateTime TimeCreated { get; init; }
    public required DateTime TimeModified { get; init; }
    public required DateTime? TimeLastPlayed { get; init; }
}
