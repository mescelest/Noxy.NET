using Noxy.NET.MediaPlayer.Domain.Abstractions.Entities;

namespace Noxy.NET.MediaPlayer.Domain.Entities;

public class EntityMediaFileGenre : BaseEntity
{
    public required Guid MediaFileID { get; set; }
    public required Guid GenreID { get; set; }

    public required int Position { get; set; }

    // Only loaded when a media file is read with its links.
    public EntityGenre? Genre { get; set; }
}
