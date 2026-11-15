using Noxy.NET.MediaPlayer.Domain.Abstractions.Entities;
using Noxy.NET.MediaPlayer.Domain.Interfaces;

namespace Noxy.NET.MediaPlayer.Domain.Entities;

public class EntityArtist : BaseEntity, IEntityNamedItem
{
    public required Guid UserID { get; set; }

    public required string Name { get; set; }
    public required string NameKey { get; set; }

    public DateTime TimeModified { get; set; } = DateTime.UtcNow;
}
