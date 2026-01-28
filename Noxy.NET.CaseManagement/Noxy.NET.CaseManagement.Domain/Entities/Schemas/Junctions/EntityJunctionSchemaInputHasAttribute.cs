using Noxy.NET.CaseManagement.Domain.Abstractions.Entities;
using Noxy.NET.CaseManagement.Domain.Interfaces;

namespace Noxy.NET.CaseManagement.Domain.Entities.Schemas.Junctions;

public class EntityJunctionSchemaInputHasAttribute : BaseEntityManyToMany<EntitySchemaInput, EntitySchemaAttribute>, IOrderedEntity
{
    public required int Order { get; set; }
    
    public override string ToString()
    {
        return Relation?.Name ?? ID.ToString();
    }
}
