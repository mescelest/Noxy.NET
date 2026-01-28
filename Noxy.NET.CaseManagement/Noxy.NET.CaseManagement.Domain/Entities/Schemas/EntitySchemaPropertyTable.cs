using Noxy.NET.CaseManagement.Domain.Entities.Schemas.Discriminators;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas.Junctions;

namespace Noxy.NET.CaseManagement.Domain.Entities.Schemas;

public class EntitySchemaPropertyTable : EntitySchemaProperty
{
    public ICollection<EntityJunctionSchemaPropertyTableHasProperty>? PropertyList { get; set; }
}
