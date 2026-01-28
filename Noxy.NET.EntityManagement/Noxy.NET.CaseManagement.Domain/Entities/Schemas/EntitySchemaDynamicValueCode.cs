using System.Text.Json.Serialization;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas.Discriminators;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas.Junctions;

namespace Noxy.NET.CaseManagement.Domain.Entities.Schemas;

public class EntitySchemaDynamicValueCode : EntitySchemaDynamicValue
{
    public required string Value { get; set; }
    public required bool IsAsynchronous { get; set; }
    
    [JsonIgnore]
    public List<EntityJunctionSchemaActionHasDynamicValueCode>? RelationActionList { get; set; }
}
