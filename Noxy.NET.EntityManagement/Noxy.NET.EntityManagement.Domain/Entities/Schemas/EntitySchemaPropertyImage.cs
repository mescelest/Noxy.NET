using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Discriminators;

namespace Noxy.NET.EntityManagement.Domain.Entities.Schemas;

public class EntitySchemaPropertyImage : EntitySchemaProperty
{
    public required string AllowedExtensions { get; set; }
}
