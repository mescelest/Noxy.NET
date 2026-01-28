using System.Text.Json.Serialization;
using Noxy.NET.EntityManagement.Domain.Abstractions.Entities;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Junctions;

namespace Noxy.NET.EntityManagement.Domain.Entities.Schemas;

public class EntitySchemaElement : BaseEntitySchemaComponent
{
    public List<EntityJunctionSchemaElementHasProperty>? PropertyList { get; set; }

    [JsonIgnore]
    public List<EntityJunctionSchemaContextHasElement>? ContextList { get; set; }
}
