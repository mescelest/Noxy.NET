using Noxy.NET.EntityManagement.Domain.Abstractions.Entities;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Discriminators;

namespace Noxy.NET.EntityManagement.Domain.Entities.Schemas;

public class EntitySchema : BaseEntityTemplate
{
    public required bool IsActive { get; set; }
    public required DateTime? TimeActivated { get; set; }

    public List<EntitySchemaContext>? ContextList { get; set; }
    public List<EntitySchemaParameter.Discriminator>? DynamicValueList { get; set; }
    public List<EntitySchemaElement>? ElementList { get; set; }
    public List<EntitySchemaProperty.Discriminator>? PropertyList { get; set; }
}
