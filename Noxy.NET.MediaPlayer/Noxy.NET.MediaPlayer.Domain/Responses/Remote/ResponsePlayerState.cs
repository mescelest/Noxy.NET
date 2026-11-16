using Noxy.NET.MediaPlayer.Domain.Abstractions;
using Noxy.NET.MediaPlayer.Domain.Enums;

namespace Noxy.NET.MediaPlayer.Domain.Responses.Remote;

public class ResponsePlayerState : BaseResponse
{
    public required Guid? ActiveDeviceID { get; init; }
    public required long StateRevision { get; init; }
    public required bool IsPlaying { get; init; }
    public required double PositionSeconds { get; init; }
    public required DateTime ServerTime { get; init; }
    public required int Volume { get; init; }
    public required ShuffleModeEnum Shuffle { get; init; }
    public required RepeatModeEnum Repeat { get; init; }
    public required Guid? CurrentItemID { get; init; }
    public required string? Title { get; init; }
    public required string? Artist { get; init; }
}
