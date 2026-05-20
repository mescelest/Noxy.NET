namespace Noxy.NET.EntityManagement.Domain.Abstractions.Entities;

public abstract class BaseEntity
{
    public Guid ID { get; set; } = CreateID();
    public DateTime TimeCreated { get; set; } = DateTime.UtcNow;
    public DateTime? TimeUpdated { get; set; }

    public const int DefaultWeight = 0;

    public override string ToString()
    {
        return ID.ToString();
    }

    public static Guid CreateID()
    {
        return Guid.CreateVersion7();
    }

    public static string CreateTemporarySchemaIdentifier()
    {
        return Guid.CreateVersion7().ToString("N");
    }
}
