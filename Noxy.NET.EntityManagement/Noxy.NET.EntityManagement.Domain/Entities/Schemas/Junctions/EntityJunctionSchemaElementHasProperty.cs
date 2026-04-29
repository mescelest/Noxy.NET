using Noxy.NET.EntityManagement.Domain.Abstractions.Entities;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Discriminators;

namespace Noxy.NET.EntityManagement.Domain.Entities.Schemas.Junctions;

public class EntityJunctionSchemaElementHasProperty : BaseEntityManyToMany<EntitySchemaElement, EntitySchemaProperty.Discriminator>
{
    public override string ToString()
    {
        return Relation?.GetValue().Name ?? ID.ToString();
    }
}
