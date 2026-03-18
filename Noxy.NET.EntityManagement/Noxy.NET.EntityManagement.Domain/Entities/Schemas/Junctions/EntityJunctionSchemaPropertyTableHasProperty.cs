using Noxy.NET.EntityManagement.Domain.Abstractions.Entities;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Discriminators;

namespace Noxy.NET.EntityManagement.Domain.Entities.Schemas.Junctions;

public class EntityJunctionSchemaPropertyTableHasProperty : BaseEntityManyToMany<EntitySchemaPropertyTable, EntitySchemaProperty.Discriminator>
{
    public required FeatureOrdering Ordering { get; set; }

    public override string ToString()
    {
        return Relation?.GetValue().Description.Name ?? ID.ToString();
    }
}
