namespace Noxy.NET.EntityManagement.Domain.Abstractions.Entities;

public abstract class BaseEntityData : BaseEntity
{
    public required string SchemaIdentifier { get; set; }
}
