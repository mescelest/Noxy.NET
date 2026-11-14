using Noxy.NET.MediaPlayer.Domain.Abstractions;

namespace Noxy.NET.MediaPlayer.Domain.Responses.Media;

public class ResponseUploadAccepted : BaseResponse
{
    public required Guid JobID { get; init; }
}
