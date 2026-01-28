using Noxy.NET.EntityManagement.Domain.Abstractions.Entities;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Discriminators;
using Noxy.NET.EntityManagement.Domain.Interfaces;

namespace Noxy.NET.EntityManagement.Domain.Entities.Schemas.Junctions;

public class EntityJunctionSchemaPropertyTableHasProperty : BaseEntityManyToMany<EntitySchemaPropertyTable, EntitySchemaProperty.Discriminator>, IOrderedEntity
{
    public required int Order { get; set; }

    public override string ToString()
    {
        return Relation?.GetValue().Name ?? ID.ToString();
    }
}
