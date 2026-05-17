using Noxy.NET.EntityManagement.Domain.Abstractions;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Discriminators;

namespace Noxy.NET.EntityManagement.Domain.Responses.Schema.Parameter;

public class ResponseSchemaParameterFind(EntitySchemaParameter.Discriminator value) : BaseResponse
{
    public EntitySchemaParameter.Discriminator Value { get; } = value;
}
