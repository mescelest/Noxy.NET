using Noxy.NET.MediaPlayer.Domain.Abstractions;
using Noxy.NET.MediaPlayer.Domain.Declarations;

namespace Noxy.NET.MediaPlayer.Domain.Responses.Metadata;

public class ResponseGenre : BaseResponse
{
    public required Guid ID { get; init; }
    public required string Name { get; init; }
    public required int MediaFileCount { get; init; }
    public int Weight { get; init; } = MetadataWeight.Default;
    public required DateTime TimeModified { get; init; }
}
