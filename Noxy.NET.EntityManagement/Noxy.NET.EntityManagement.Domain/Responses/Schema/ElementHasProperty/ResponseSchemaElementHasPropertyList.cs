using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Junctions;

namespace Noxy.NET.EntityManagement.Domain.Responses.Schema.ElementHasProperty;

public class ResponseSchemaElementHasPropertyList(List<EntitySchemaElementHasProperty> value)
{
    public List<EntitySchemaElementHasProperty> Value { get; } = value;
}
