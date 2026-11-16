using Noxy.NET.MediaPlayer.Domain.Abstractions;
using Noxy.NET.MediaPlayer.Domain.Models;

namespace Noxy.NET.MediaPlayer.Domain.Responses.Remote;

public class ResponseQueue : BaseResponse
{
    public required long QueueRevision { get; init; }
    public required ResponseQueueSource? Source { get; init; }
    public required List<QueueItem> HistoryList { get; init; }
    public required QueueItem? Current { get; init; }
    public required List<QueueItem> UserQueueList { get; init; }
    public required List<QueueItem> SourceList { get; init; }
}
