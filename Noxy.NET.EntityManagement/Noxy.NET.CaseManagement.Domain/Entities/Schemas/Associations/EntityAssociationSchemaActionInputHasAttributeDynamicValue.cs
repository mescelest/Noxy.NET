using Noxy.NET.CaseManagement.Domain.Entities.Schemas.Discriminators;

namespace Noxy.NET.CaseManagement.Domain.Entities.Schemas.Associations;

public class EntityAssociationSchemaActionInputHasAttributeDynamicValue : EntityAssociationSchemaActionInputHasAttribute
{
    public EntitySchemaDynamicValue.Discriminator? Value { get; set; }
    public Guid? ValueID { get; set; }
}
