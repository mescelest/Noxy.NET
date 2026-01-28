using Noxy.NET.CaseManagement.Domain.Entities.Schemas.Discriminators;
using Noxy.NET.CaseManagement.Domain.Enums;

namespace Noxy.NET.CaseManagement.Domain.Entities.Schemas;

public class EntitySchemaDynamicValueTextParameter : EntitySchemaDynamicValueParameter
{
    public required TextParameterTypeEnum Type { get; set; }
}
