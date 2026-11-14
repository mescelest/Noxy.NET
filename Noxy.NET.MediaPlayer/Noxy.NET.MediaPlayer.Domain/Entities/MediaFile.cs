using Noxy.NET.MediaPlayer.Domain.Abstractions.Entities;

namespace Noxy.NET.MediaPlayer.Domain.Entities;

public class MediaFile : BaseEntity
{
    public required string FilePath { get; set; }

    public required string ContentType { get; set; }
    public required long SizeBytes { get; set; }

    // SHA-256 of the file as lowercase hex.
    public required string ContentHash { get; set; }

    public DateTime TimeModified { get; set; } = DateTime.UtcNow;

    public required Guid UserID { get; set; }

    public MediaFileMetadata? Metadata { get; set; }
}
