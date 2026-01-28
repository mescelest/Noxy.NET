using System.Text.Json.Serialization;
using Noxy.NET.CaseManagement.Domain.Abstractions.Entities;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas.Junctions;

namespace Noxy.NET.CaseManagement.Domain.Entities.Schemas;

public class EntitySchemaActionStep : BaseEntitySchemaComponent
{
    public required bool IsRepeatable { get; set; }
    
    public List<EntityJunctionSchemaActionStepHasActionInput>? ActionInputList { get; set; }
    
    [JsonIgnore]
    public List<EntityJunctionSchemaActionHasActionStep>? ActionList { get; set; }
}
