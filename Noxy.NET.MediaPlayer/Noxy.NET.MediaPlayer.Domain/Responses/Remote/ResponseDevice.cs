using Noxy.NET.MediaPlayer.Domain.Abstractions;

namespace Noxy.NET.MediaPlayer.Domain.Responses.Remote;

public class ResponseDevice : BaseResponse
{
    public required Guid DeviceID { get; init; }
    public required string Name { get; init; }
    public required bool IsActive { get; init; }
}
