using Noxy.NET.MediaPlayer.Domain.Abstractions.Entities;

namespace Noxy.NET.MediaPlayer.Domain.Entities;

public class EntityMediaFileArtist : BaseEntity
{
    public required Guid MediaFileID { get; set; }
    public required Guid ArtistID { get; set; }

    public required int Position { get; set; }

    // Only loaded when a media file is read with its links.
    public EntityArtist? Artist { get; set; }
}
