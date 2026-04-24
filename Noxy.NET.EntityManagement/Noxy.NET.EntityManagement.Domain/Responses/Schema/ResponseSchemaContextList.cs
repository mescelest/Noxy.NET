using Noxy.NET.EntityManagement.Domain.Entities.Schemas;

namespace Noxy.NET.EntityManagement.Domain.Responses.Schema;

public class ResponseSchemaContextList
{
    public required List<EntitySchemaContext> Value { get; set; }
}
