using Noxy.NET.EntityManagement.Domain.Entities.Schemas;

namespace Noxy.NET.EntityManagement.Domain.Responses.Schema;

public class ResponseSchemaList
{
    public required List<EntitySchema> Value { get; set; }
}
