using Noxy.NET.EntityManagement.Domain.Entities.Schemas;

namespace Noxy.NET.EntityManagement.Domain.Responses.Schema.Context;

public class ResponseSchemaContextList(List<EntitySchemaContext> value)
{
    public List<EntitySchemaContext> Value { get; } = value;
}
