using System.ComponentModel.DataAnnotations;
using Noxy.NET.MediaPlayer.Domain.Enums;

namespace Noxy.NET.MediaPlayer.Application.Requests.Remote;

public class RequestReportPlayerState
{
    public required long StateRevision { get; init; }
    public required bool IsPlaying { get; init; }

    [Range(0, double.MaxValue)]
    public required double PositionSeconds { get; init; }

    [Range(0, 100)]
    public required int Volume { get; init; }

    public required ShuffleModeEnum Shuffle { get; init; }
    public required RepeatModeEnum Repeat { get; init; }
    public Guid? CurrentItemID { get; init; }

    [MaxLength(500)]
    public string? Title { get; init; }

    [MaxLength(500)]
    public string? Artist { get; init; }
}
