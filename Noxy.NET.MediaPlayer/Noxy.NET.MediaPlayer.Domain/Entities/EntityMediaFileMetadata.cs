using Noxy.NET.MediaPlayer.Domain.Abstractions.Entities;

namespace Noxy.NET.MediaPlayer.Domain.Entities;

public class EntityMediaFileMetadata : BaseEntity
{
    public required string Title { get; set; }
    public string? Subtitle { get; set; }
    public string? Comment { get; set; }
    public int? TrackNumber { get; set; }
    public int? DiscNumber { get; set; }
    public int? Year { get; set; }
    public TimeSpan? Duration { get; set; }

    public required Guid MediaFileID { get; set; }
}
