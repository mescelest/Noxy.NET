using Noxy.NET.EntityManagement.Domain.Entities.Schemas;

namespace Noxy.NET.EntityManagement.Domain.Responses.Schema.Element;

public class ResponseSchemaElementList(List<EntitySchemaElement> value)
{
    public List<EntitySchemaElement> Value { get; } = value;
}
