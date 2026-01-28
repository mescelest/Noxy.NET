using System.Text.Json.Serialization;
using Noxy.NET.CaseManagement.Domain.Abstractions.Entities;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas.Junctions;

namespace Noxy.NET.CaseManagement.Domain.Entities.Schemas;

public class EntitySchemaInput : BaseEntitySchema
{
    public List<EntityJunctionSchemaInputHasAttribute>? AttributeList { get; set; }

    [JsonIgnore]
    public List<EntitySchemaActionInput>? ActionInputList { get; set; }
}
