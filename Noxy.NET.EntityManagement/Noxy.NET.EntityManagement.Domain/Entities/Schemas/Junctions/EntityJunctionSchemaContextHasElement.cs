using Noxy.NET.EntityManagement.Domain.Abstractions.Entities;
using Noxy.NET.EntityManagement.Domain.Interfaces;

namespace Noxy.NET.EntityManagement.Domain.Entities.Schemas.Junctions;

public class EntityJunctionSchemaContextHasElement : BaseEntityManyToMany<EntitySchemaContext, EntitySchemaElement>, IOrderedEntity
{
    public required int Order { get; set; }

    public override string ToString()
    {
        return Relation?.Name ?? ID.ToString();
    }
}
