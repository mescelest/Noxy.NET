using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Discriminators;
using Noxy.NET.EntityManagement.Domain.Enums;

namespace Noxy.NET.EntityManagement.Domain.Entities.Schemas;

public class EntitySchemaDynamicValueTextParameter : EntitySchemaDynamicValueParameter
{
    public required TextParameterTypeEnum Type { get; set; }
}
