using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Discriminators;

namespace Noxy.NET.EntityManagement.Domain.Responses.Schema;

public class ResponseSchemaParameterList
{
    public required List<EntitySchemaParameter.Discriminator> Value { get; set; }
}
