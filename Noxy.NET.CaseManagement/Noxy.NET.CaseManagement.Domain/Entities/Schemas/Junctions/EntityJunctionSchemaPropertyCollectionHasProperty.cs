using Noxy.NET.CaseManagement.Domain.Abstractions.Entities;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas.Discriminators;
using Noxy.NET.CaseManagement.Domain.Interfaces;

namespace Noxy.NET.CaseManagement.Domain.Entities.Schemas.Junctions;

public class EntityJunctionSchemaPropertyCollectionHasProperty : BaseEntityManyToMany<EntitySchemaPropertyCollection, EntitySchemaProperty.Discriminator>, IOrderedEntity
{
    public required int Order { get; set; }

    public override string ToString()
    {
        return Relation?.GetValue().Name ?? ID.ToString();
    }

}
