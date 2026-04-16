using System.ComponentModel;
using Noxy.NET.EntityManagement.Domain.Abstractions.Entities;
using Noxy.NET.EntityManagement.Domain.Constants;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Discriminators;

namespace Noxy.NET.EntityManagement.Domain.Entities.Schemas.Junctions;

public class EntityJunctionSchemaPropertyCollectionHasProperty : BaseEntityManyToMany<EntitySchemaPropertyCollection, EntitySchemaProperty.Discriminator>
{
    [DisplayName(TextConstants.LabelFormOrder)]
    [Description(TextConstants.HelpFormOrder)]
    public int Order { get; set; } = DefaultOrder;

    public override string ToString()
    {
        return Relation?.GetValue().Name ?? ID.ToString();
    }
}
