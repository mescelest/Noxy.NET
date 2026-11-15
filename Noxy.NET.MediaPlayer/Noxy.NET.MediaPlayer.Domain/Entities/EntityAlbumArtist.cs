using Noxy.NET.MediaPlayer.Domain.Abstractions.Entities;

namespace Noxy.NET.MediaPlayer.Domain.Entities;

public class EntityAlbumArtist : BaseEntity
{
    public required Guid AlbumID { get; set; }
    public required Guid ArtistID { get; set; }

    public required int Position { get; set; }

    // Only loaded when an album is read with its artists.
    public EntityArtist? Artist { get; set; }
}
