using Noxy.NET.CaseManagement.Domain.Abstractions.Entities;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas.Junctions;

namespace Noxy.NET.CaseManagement.Domain.Entities.Schemas;

public class EntitySchemaContext : BaseEntitySchemaComponent
{
    public List<EntityJunctionSchemaContextHasAction>? ActionList { get; set; }
    public List<EntityJunctionSchemaContextHasElement>? ElementList { get; set; }
}
