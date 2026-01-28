using Noxy.NET.EntityManagement.Domain.Abstractions.Entities;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Junctions;

namespace Noxy.NET.EntityManagement.Domain.Entities.Schemas;

public class EntitySchemaContext : BaseEntitySchemaComponent
{
    public List<EntityJunctionSchemaContextHasElement>? ElementList { get; set; }
}
