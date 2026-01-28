namespace Noxy.NET.EntityManagement.Domain.Entities.Schemas.Discriminators;

public class EntitySchemaDynamicValueParameter : EntitySchemaDynamicValue
{
    public required bool IsApprovalRequired { get; set; }
}
