using System.ComponentModel.DataAnnotations;
using Noxy.NET.MediaPlayer.Domain.Responses.Remote;

namespace Noxy.NET.MediaPlayer.Application.Requests.Remote;

public class RequestClaimActive
{
    [Required]
    public required RequestReportPlayerState State { get; init; }

    [Required]
    public required ResponseQueue Queue { get; init; }
}
