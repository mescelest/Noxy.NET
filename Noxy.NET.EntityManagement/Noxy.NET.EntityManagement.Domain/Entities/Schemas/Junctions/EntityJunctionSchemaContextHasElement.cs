using Noxy.NET.EntityManagement.Domain.Abstractions.Entities;

namespace Noxy.NET.EntityManagement.Domain.Entities.Schemas.Junctions;

public class EntityJunctionSchemaContextHasElement : BaseEntityManyToMany<EntitySchemaContext, EntitySchemaElement>
{
    public required FeatureOrdering Ordering { get; set; }

    public override string ToString()
    {
        return Relation?.Description.Name ?? ID.ToString();
    }
}
