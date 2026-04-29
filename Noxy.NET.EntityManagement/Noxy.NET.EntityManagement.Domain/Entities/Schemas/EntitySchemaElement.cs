using System.Text.Json.Serialization;
using Noxy.NET.EntityManagement.Domain.Abstractions.Entities;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas.Junctions;
using Noxy.NET.EntityManagement.Domain.Interfaces;

namespace Noxy.NET.EntityManagement.Domain.Entities.Schemas;

public class EntitySchemaElement : BaseEntitySchema, ISchemaMetadata, ISchemaPresentation, ISchemaOrdering
{
    public required string Name { get; set; }
    public string Note { get; set; } = string.Empty;
    public int Weight { get; set; } = DefaultWeight;

    public EntitySchemaParameterText? TitleTextParameter { get; set; }
    public Guid TitleTextParameterID { get; set; }

    public EntitySchemaParameterText? DescriptionTextParameter { get; set; }
    public Guid? DescriptionTextParameterID { get; set; }

    public List<EntityJunctionSchemaElementHasProperty>? PropertyList { get; set; }

    [JsonIgnore]
    public List<EntityJunctionSchemaContextHasElement>? ContextList { get; set; }
}
