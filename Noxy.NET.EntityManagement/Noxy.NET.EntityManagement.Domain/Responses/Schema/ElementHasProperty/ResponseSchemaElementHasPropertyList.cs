using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Junctions;

namespace Noxy.NET.EntityManagement.Domain.Responses.Schema.ElementHasProperty;

public class ResponseSchemaElementHasPropertyList
{
    public required List<EntitySchemaElementHasProperty> Value { get; set; }
}
