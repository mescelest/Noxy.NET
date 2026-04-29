using Noxy.NET.EntityManagement.Domain.Abstractions.Entities;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Discriminators;
using Noxy.NET.EntityManagement.Domain.Interfaces;

namespace Noxy.NET.EntityManagement.Domain.Entities.Schemas;

public class EntitySchema : BaseEntity, ISchemaMetadata
{
    public required string Name { get; set; }
    public string Note { get; set; } = string.Empty;
    public required bool IsActive { get; set; }
    public required DateTime? TimeActivated { get; set; }

    public List<EntitySchemaContext>? ContextList { get; set; }
    public List<EntitySchemaParameter.Discriminator>? ParameterList { get; set; }
    public List<EntitySchemaElement>? ElementList { get; set; }
    public List<EntitySchemaProperty.Discriminator>? PropertyList { get; set; }
}
