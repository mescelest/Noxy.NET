using Noxy.NET.EntityManagement.Domain.Entities.Schemas;

namespace Noxy.NET.EntityManagement.Domain.Responses.Schema.Element;

public class ResponseSchemaElementFind(EntitySchemaElement value)
{
    public EntitySchemaElement Value { get; } = value;
}
