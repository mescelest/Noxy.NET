using Noxy.NET.CaseManagement.Domain.Entities.Schemas.Discriminators;
using Noxy.NET.CaseManagement.Domain.Enums;

namespace Noxy.NET.CaseManagement.Domain.Entities.Schemas;

public class EntitySchemaPropertyDateTime : EntitySchemaProperty.Primitive
{
    public required DateTimeTypeEnum Type { get; set; }
    
    public EntitySchemaDynamicValueCode? MinDynamic { get; set; }
    public required Guid? MinDynamicID { get; set; }

    public EntitySchemaDynamicValueCode? MaxDynamic { get; set; }
    public required Guid? MaxDynamicID { get; set; }
}
