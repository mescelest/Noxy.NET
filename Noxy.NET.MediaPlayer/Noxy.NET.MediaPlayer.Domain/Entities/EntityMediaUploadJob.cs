using Noxy.NET.MediaPlayer.Domain.Abstractions.Entities;
using Noxy.NET.MediaPlayer.Domain.Enums;

namespace Noxy.NET.MediaPlayer.Domain.Entities;

public class EntityMediaUploadJob : BaseEntity
{
    public required Guid UserID { get; set; }

    public required string StagedFileName { get; set; }
    public required string Title { get; set; }

    public required string ContentType { get; set; }
    public required long SizeBytes { get; set; }
    public required string ContentHash { get; set; }

    public MediaUploadJobStatus Status { get; set; } = MediaUploadJobStatus.Pending;
}
