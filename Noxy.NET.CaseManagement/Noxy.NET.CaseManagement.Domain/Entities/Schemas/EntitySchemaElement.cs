using System.Text.Json.Serialization;
using Noxy.NET.CaseManagement.Domain.Abstractions.Entities;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas.Junctions;

namespace Noxy.NET.CaseManagement.Domain.Entities.Schemas;

public class EntitySchemaElement : BaseEntitySchemaComponent
{
    public List<EntityJunctionSchemaElementHasAction>? ActionList { get; set; }
    public List<EntityJunctionSchemaElementHasProperty>? PropertyList { get; set; }

    [JsonIgnore]
    public List<EntityJunctionSchemaContextHasElement>? ContextList { get; set; }
}
