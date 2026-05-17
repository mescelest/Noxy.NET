using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Junctions;

namespace Noxy.NET.EntityManagement.Domain.Responses.Schema.ContextHasElement;

public class ResponseSchemaContextHasElementList(List<EntitySchemaContextHasElement> value)
{
    public List<EntitySchemaContextHasElement> Value { get; } = value;
}
