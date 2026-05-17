using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Discriminators;

namespace Noxy.NET.EntityManagement.Domain.Responses.Schema.Property;

public class ResponseSchemaPropertyList(List<EntitySchemaProperty.Discriminator> value)
{
    public List<EntitySchemaProperty.Discriminator> Value { get; } = value;
}
