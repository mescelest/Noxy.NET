using Noxy.NET.EntityManagement.Domain.Abstractions.Entities;

namespace Noxy.NET.EntityManagement.Domain.Entities.Schemas.Junctions;

public class EntitySchemaContextHasElement : BaseEntityManyToMany<EntitySchemaContext, EntitySchemaElement>
{
    public override string ToString()
    {
        return Relation?.Name ?? ID.ToString();
    }
}
