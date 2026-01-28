using Noxy.NET.CaseManagement.Domain.Abstractions.Entities;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas.Discriminators;

namespace Noxy.NET.CaseManagement.Domain.Entities.Schemas;

public class EntitySchema : BaseEntityTemplate
{
    public required bool IsActive { get; set; }
    public required DateTime? TimeActivated { get; set; }

    public List<EntitySchemaAction>? ActionList { get; set; }
    public List<EntitySchemaActionInput>? ActionInputList { get; set; }
    public List<EntitySchemaActionStep>? ActionStepList { get; set; }
    public List<EntitySchemaAttribute>? AttributeList { get; set; }
    public List<EntitySchemaContext>? ContextList { get; set; }
    public List<EntitySchemaDynamicValue.Discriminator>? DynamicValueList { get; set; }
    public List<EntitySchemaElement>? ElementList { get; set; }
    public List<EntitySchemaInput>? InputList { get; set; }
    public List<EntitySchemaProperty.Discriminator>? PropertyList { get; set; }
}
