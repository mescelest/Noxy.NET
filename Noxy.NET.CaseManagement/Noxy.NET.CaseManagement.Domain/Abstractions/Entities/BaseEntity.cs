namespace Noxy.NET.CaseManagement.Domain.Abstractions.Entities;

public abstract class BaseEntity
{
    public Guid ID { get; init; } = CreateID();
    public DateTime? TimeCreated { get; set; } = DateTime.UtcNow;
    
    public override string ToString()
    {
        return ID.ToString();
    }

    public static Guid CreateID()
    {
        return Guid.CreateVersion7();
    }
}