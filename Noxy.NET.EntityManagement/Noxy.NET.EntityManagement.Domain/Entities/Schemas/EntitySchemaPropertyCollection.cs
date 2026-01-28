using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Discriminators;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Junctions;

namespace Noxy.NET.EntityManagement.Domain.Entities.Schemas;

public class EntitySchemaPropertyCollection : EntitySchemaProperty
{
    public ICollection<EntityJunctionSchemaPropertyCollectionHasProperty>? PropertyList { get; set; }
}
