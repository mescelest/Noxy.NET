using Noxy.NET.EntityManagement.Domain.Abstractions.Entities;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Junctions;
using Noxy.NET.EntityManagement.Domain.Interfaces;

namespace Noxy.NET.EntityManagement.Domain.Entities.Schemas;

public class EntitySchemaContext : BaseEntitySchemaPresentation, ISchemaMetadata
{
    public required string Name { get; set; }
    public string Note { get; set; } = string.Empty;

    public List<EntityJunctionSchemaContextHasElement>? ElementList { get; set; }
}
