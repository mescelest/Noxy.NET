using Noxy.NET.EntityManagement.Domain.Abstractions;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Discriminators;

namespace Noxy.NET.EntityManagement.Domain.Responses.Schema.Property;

public class ResponseSchemaPropertyFind(EntitySchemaProperty.Discriminator value) : BaseResponse
{
    public EntitySchemaProperty.Discriminator Value { get; } = value;
}
