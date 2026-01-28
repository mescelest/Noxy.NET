using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Discriminators;

namespace Noxy.NET.EntityManagement.Domain.Entities.Schemas;

public class EntitySchemaPropertyInteger : EntitySchemaProperty.Primitive
{
    public required bool IsUnsigned { get; set; } = false;
}
