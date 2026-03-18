using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Discriminators;
using Noxy.NET.EntityManagement.Domain.Enums;

namespace Noxy.NET.EntityManagement.Domain.Entities.Schemas;

public class EntitySchemaPropertyDateTime : EntitySchemaProperty
{
    public required DateTimeTypeEnum Type { get; set; }
}
