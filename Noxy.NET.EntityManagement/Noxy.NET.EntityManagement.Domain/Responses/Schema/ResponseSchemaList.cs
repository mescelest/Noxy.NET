using Noxy.NET.EntityManagement.Domain.Abstractions;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;

namespace Noxy.NET.EntityManagement.Domain.Responses.Schema;

public class ResponseSchemaList(List<EntitySchema> value) : BaseResponse
{
    public List<EntitySchema> Value { get; } = value;
}
