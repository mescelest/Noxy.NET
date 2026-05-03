using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Discriminators;

namespace Noxy.NET.EntityManagement.Domain.Responses.Schema.Property;

public class ResponseSchemaPropertyList
{
    public required List<EntitySchemaProperty.Discriminator> Value { get; set; }
}
