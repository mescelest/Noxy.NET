using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Discriminators;
using Noxy.NET.EntityManagement.Domain.Enums;

namespace Noxy.NET.EntityManagement.Domain.Entities.Schemas;

public class EntitySchemaParameterText : EntitySchemaParameter
{
    public required TextParameterTypeEnum Type { get; set; }
}
