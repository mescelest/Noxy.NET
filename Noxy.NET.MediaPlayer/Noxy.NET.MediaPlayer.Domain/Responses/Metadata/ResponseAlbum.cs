using Noxy.NET.MediaPlayer.Domain.Abstractions;

namespace Noxy.NET.MediaPlayer.Domain.Responses.Metadata;

public class ResponseAlbum : BaseResponse
{
    public required Guid ID { get; init; }
    public required string Title { get; init; }
    public required List<Guid> ArtistIDList { get; init; }
    public required int MediaFileCount { get; init; }
    public required DateTime TimeModified { get; init; }
}
