using System.Text.Json.Serialization;
using Noxy.NET.CaseManagement.Domain.Abstractions.Entities;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas.Junctions;

namespace Noxy.NET.CaseManagement.Domain.Entities.Schemas;

public class EntitySchemaAction : BaseEntitySchemaComponent
{
    public EntitySchemaDynamicValueCode? IsActiveDynamic { get; set; }
    public Guid? IsActiveDynamicID { get; set; }
    
    public List<EntityJunctionSchemaActionHasActionStep>? ActionStepList { get; set; }
    public List<EntityJunctionSchemaActionHasDynamicValueCode>? DynamicValueCodeList { get; set; }

    [JsonIgnore]
    public List<EntityJunctionSchemaElementHasAction>? ActionList { get; set; }
    
    [JsonIgnore]
    public List<EntityJunctionSchemaContextHasAction>? ContextList { get; set; }
}
