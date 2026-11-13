namespace Noxy.NET.MediaPlayer.Domain.Abstractions.Entities;

public abstract class BaseEntity
{
    public Guid ID { get; init; } = Guid.CreateVersion7();
    public DateTime TimeCreated { get; init; } = DateTime.UtcNow;
}
