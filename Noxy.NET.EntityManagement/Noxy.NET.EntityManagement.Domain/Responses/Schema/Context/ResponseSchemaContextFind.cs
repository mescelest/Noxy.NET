using Noxy.NET.EntityManagement.Domain.Entities.Schemas;

namespace Noxy.NET.EntityManagement.Domain.Responses.Schema.Context;

public class ResponseSchemaContextFind(EntitySchemaContext value)
{
    public EntitySchemaContext Value { get; } = value;
}
