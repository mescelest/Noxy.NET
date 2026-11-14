using Noxy.NET.MediaPlayer.Domain.Abstractions;
using Noxy.NET.MediaPlayer.Domain.Enums;

namespace Noxy.NET.MediaPlayer.Domain.Responses.Media;

public class ResponseUploadResult : BaseResponse
{
    public required Guid JobID { get; init; }
    public required string Title { get; init; }
    public required bool Succeeded { get; init; }

    public ResponseMediaFile? Media { get; init; }
    public UploadFailureReason? Reason { get; init; }
}
