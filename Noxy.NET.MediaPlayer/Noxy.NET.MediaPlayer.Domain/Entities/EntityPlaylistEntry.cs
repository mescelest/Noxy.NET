using Noxy.NET.MediaPlayer.Domain.Abstractions.Entities;

namespace Noxy.NET.MediaPlayer.Domain.Entities;

public class EntityPlaylistEntry : BaseEntity
{
    public required Guid PlaylistID { get; set; }
    public required Guid MediaFileID { get; set; }

    public required int Position { get; set; }

    // Only loaded when a playlist is read with its entries.
    public EntityMediaFile? MediaFile { get; set; }
}
