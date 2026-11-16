using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Models;

namespace Noxy.NET.MediaPlayer.Application.Models.Remote;

public class LiveSession
{
    public required Guid UserID { get; init; }
    public required LiveQueue Queue { get; set; }

    public Guid? ActiveDeviceID { get; set; }
    public long StateRevision { get; set; }
    public bool IsPlaying { get; set; }
    public double PositionSeconds { get; set; }
    public DateTime StateTime { get; set; }
    public int Volume { get; set; }
    public ShuffleModeEnum Shuffle { get; set; }
    public RepeatModeEnum Repeat { get; set; }
    public Guid? CurrentItemID { get; set; }
    public string? Title { get; set; }
    public string? Artist { get; set; }

    // The active device dropped and its record is kept for the grace period.
    public ITimer? GraceTimer { get; set; }
    public long GraceVersion { get; set; }
    public ITimer? IdleTimer { get; set; }
    public long IdleVersion { get; set; }

    // A transfer to another device is under way: only one at a time.
    public bool TransferPending { get; set; }
    public Guid? TransferTargetID { get; set; }
    public TaskCompletionSource? FinalStateSource { get; set; }
    public TaskCompletionSource? ClaimSource { get; set; }
}
