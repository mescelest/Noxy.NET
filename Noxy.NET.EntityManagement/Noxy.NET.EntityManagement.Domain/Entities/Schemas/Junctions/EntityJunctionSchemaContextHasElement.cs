using System.ComponentModel;
using Noxy.NET.EntityManagement.Domain.Abstractions.Entities;
using Noxy.NET.EntityManagement.Domain.Constants;

namespace Noxy.NET.EntityManagement.Domain.Entities.Schemas.Junctions;

public class EntityJunctionSchemaContextHasElement : BaseEntityManyToMany<EntitySchemaContext, EntitySchemaElement>
{
    [DisplayName(TextConstants.LabelFormOrder)]
    [Description(TextConstants.HelpFormOrder)]
    public int Order { get; set; } = DefaultOrder;

    public override string ToString()
    {
        return Relation?.Name ?? ID.ToString();
    }
}
