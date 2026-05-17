using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Discriminators;

namespace Noxy.NET.EntityManagement.Domain.Responses.Schema.Parameter;

public class ResponseSchemaParameterList(List<EntitySchemaParameter.Discriminator> value)
{
    public List<EntitySchemaParameter.Discriminator> Value { get; } = value;
}
