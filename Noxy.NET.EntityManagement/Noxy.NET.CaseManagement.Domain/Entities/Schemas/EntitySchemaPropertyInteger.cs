using Noxy.NET.CaseManagement.Domain.Entities.Schemas.Discriminators;

namespace Noxy.NET.CaseManagement.Domain.Entities.Schemas;

public class EntitySchemaPropertyInteger : EntitySchemaProperty.Primitive
{
    public required bool IsUnsigned { get; set; } = false;
    
    public EntitySchemaDynamicValueCode? MinDynamic { get; set; }
    public required Guid? MinDynamicID { get; set; }

    public EntitySchemaDynamicValueCode? MaxDynamic { get; set; }
    public required Guid? MaxDynamicID { get; set; }
}
