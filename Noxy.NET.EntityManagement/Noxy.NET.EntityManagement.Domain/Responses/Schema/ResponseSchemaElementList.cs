using Noxy.NET.EntityManagement.Domain.Entities.Schemas;

namespace Noxy.NET.EntityManagement.Domain.Responses.Schema;

public class ResponseSchemaElementList
{
    public required List<EntitySchemaElement> Value { get; set; }
}
