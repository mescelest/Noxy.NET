using Noxy.NET.MediaPlayer.Domain.Abstractions.Entities;
using Noxy.NET.MediaPlayer.Domain.Declarations;
using Noxy.NET.MediaPlayer.Domain.Interfaces;

namespace Noxy.NET.MediaPlayer.Domain.Entities;

public class EntityGenre : BaseEntity, IEntityNamedItem
{
    public required Guid UserID { get; set; }

    public required string Name { get; set; }
    public required string NameKey { get; set; }

    // Where the genre stands in the list of a song: higher first (MetadataWeight).
    public int Weight { get; set; } = MetadataWeight.Default;

    public DateTime TimeModified { get; set; } = DateTime.UtcNow;
}
