using Noxy.NET.MediaPlayer.Domain.Abstractions;

namespace Noxy.NET.MediaPlayer.Domain.Responses.Metadata;

public class ResponseArtist : BaseResponse
{
    public required Guid ID { get; init; }
    public required string Name { get; init; }
    public required int MediaFileCount { get; init; }
    public required DateTime TimeModified { get; init; }
}
