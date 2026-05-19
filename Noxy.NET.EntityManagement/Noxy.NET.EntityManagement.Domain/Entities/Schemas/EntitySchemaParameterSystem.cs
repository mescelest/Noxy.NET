using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Discriminators;
using Noxy.NET.EntityManagement.Domain.Enums;

namespace Noxy.NET.EntityManagement.Domain.Entities.Schemas;

public class EntitySchemaParameterSystem : EntitySchemaParameter
{
    public required ParameterSystemTypeEnum Type { get; set; }
    public required bool IsPublic { get; set; }
}
