using Noxy.NET.MediaPlayer.Domain.Abstractions.Entities;

namespace Noxy.NET.MediaPlayer.Domain.Entities;

public abstract class EntityUserSetting : BaseEntity
{
    public required Guid UserID { get; set; }

    public required string Key { get; set; }

    public DateTime TimeModified { get; set; } = DateTime.UtcNow;
}
