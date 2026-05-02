using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Junctions;

namespace Noxy.NET.EntityManagement.Domain.Responses.Schema.ContextHasElement;

public class ResponseSchemaContextHasElementList
{
    public required List<EntitySchemaContextHasElement> Value { get; set; }
}
