using Noxy.NET.MediaPlayer.Domain.Abstractions.Entities;
using Noxy.NET.MediaPlayer.Domain.Enums;

namespace Noxy.NET.MediaPlayer.Domain.Entities;

public class EntityMediaUploadJob : BaseEntity
{
    public required Guid UserID { get; set; }

    public required string StagedFileName { get; set; }
    public required string Title { get; set; }

    // False when Title is only the file name the worker falls back to, so an embedded title may win.
    public bool TitleWasSent { get; set; }

    public required string ContentType { get; set; }
    public required long SizeBytes { get; set; }
    public required string ContentHash { get; set; }

    public MediaUploadJobStatusEnum Status { get; set; } = MediaUploadJobStatusEnum.Pending;
}
