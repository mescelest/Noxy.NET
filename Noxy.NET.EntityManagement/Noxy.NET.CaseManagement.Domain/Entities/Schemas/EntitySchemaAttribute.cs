using System.Text.Json.Serialization;
using Noxy.NET.CaseManagement.Domain.Abstractions.Entities;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas.Discriminators;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas.Junctions;
using Noxy.NET.CaseManagement.Domain.Enums;

namespace Noxy.NET.CaseManagement.Domain.Entities.Schemas;

public class EntitySchemaAttribute : BaseEntitySchema
{
    public required AttributeTypeEnum Type { get; set; }
    public required bool IsValueList { get; set; }

    [JsonIgnore]
    public List<EntityJunctionSchemaInputHasAttribute>? InputList { get; set; }
    [JsonIgnore]
    public List<EntityAssociationSchemaActionInputHasAttribute.Discriminator>? ActionInputList { get; set; }

}
