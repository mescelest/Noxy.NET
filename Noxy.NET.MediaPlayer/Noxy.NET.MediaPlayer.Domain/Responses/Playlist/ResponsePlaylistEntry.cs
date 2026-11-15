using Noxy.NET.MediaPlayer.Domain.Abstractions;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;

namespace Noxy.NET.MediaPlayer.Domain.Responses.Playlist;

public class ResponsePlaylistEntry : BaseResponse
{
    public required Guid ID { get; init; }
    public required Guid MediaFileID { get; init; }
    public required int Position { get; init; }
    public required ResponseMediaFile MediaFile { get; init; }
}
