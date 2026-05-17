using Noxy.NET.EntityManagement.Domain.Abstractions;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;

namespace Noxy.NET.EntityManagement.Domain.Responses.Schema;

public class ResponseSchemaFind(EntitySchema value) : BaseResponse
{
    public EntitySchema Value { get; } = value;
}
