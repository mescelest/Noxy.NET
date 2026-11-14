using Noxy.NET.MediaPlayer.Domain.Abstractions;

namespace Noxy.NET.MediaPlayer.Domain.Responses.Media;

public class ResponseMediaFile : BaseResponse
{
    public required Guid ID { get; init; }
    public required string Title { get; init; }
    public required string ContentType { get; init; }
    public required long SizeBytes { get; init; }
    public required string ContentHash { get; init; }
    public required DateTime TimeCreated { get; init; }
    public required DateTime TimeModified { get; init; }
}
