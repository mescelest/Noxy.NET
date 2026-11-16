using Noxy.NET.MediaPlayer.Domain.Abstractions;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Models;

namespace Noxy.NET.MediaPlayer.Domain.Responses.Remote;

public class ResponseQueueDelta : BaseResponse
{
    public required long QueueRevision { get; init; }
    public required QueueEditEnum Edit { get; init; }
    public Guid? ItemID { get; init; }
    public Guid? MediaFileID { get; init; }
    public Guid? AfterItemID { get; init; }
    public QueueItem? Item { get; init; }
    public List<QueueItem>? ItemList { get; init; }
    public ResponseQueueSource? Source { get; init; }
    public Guid? CurrentItemID { get; init; }
    public bool KeepUserQueue { get; init; }
    public bool KeepSkipped { get; init; }
}
