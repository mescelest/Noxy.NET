using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Discriminators;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Junctions;

namespace Noxy.NET.EntityManagement.Domain.Entities.Schemas;

public class EntitySchemaPropertyTable : EntitySchemaProperty
{
    public ICollection<EntityJunctionSchemaPropertyTableHasProperty>? PropertyList { get; set; }
}
