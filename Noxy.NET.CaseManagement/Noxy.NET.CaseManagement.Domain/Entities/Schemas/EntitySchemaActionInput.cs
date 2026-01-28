using System.Text.Json.Serialization;
using Noxy.NET.CaseManagement.Domain.Abstractions.Entities;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas.Discriminators;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas.Junctions;

namespace Noxy.NET.CaseManagement.Domain.Entities.Schemas;

public class EntitySchemaActionInput : BaseEntitySchemaComponent
{
    public EntitySchemaDynamicValueCode? IsActiveDynamic { get; set; }
    public Guid? IsActiveDynamicID { get; set; }

    public EntitySchemaDynamicValueCode? IsValidDynamic { get; set; }
    public Guid? IsValidDynamicID { get; set; }

    public EntitySchemaInput? Input { get; set; }
    public required Guid InputID { get; set; }

    public List<EntityAssociationSchemaActionInputHasAttribute.Discriminator>? AttributeList { get; set; }

    [JsonIgnore]
    public List<EntityJunctionSchemaActionStepHasActionInput>? ActionStepList { get; set; }
}
