using Noxy.NET.CaseManagement.Domain.Entities.Schemas.Discriminators;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas.Junctions;

namespace Noxy.NET.CaseManagement.Domain.Entities.Schemas;

public class EntitySchemaPropertyCollection : EntitySchemaProperty
{
    public ICollection<EntityJunctionSchemaPropertyCollectionHasProperty>? PropertyList { get; set; }
}
