using System.ComponentModel;
using System.Text.Json.Serialization;
using Noxy.NET.CaseManagement.Domain.Constants;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas;

namespace Noxy.NET.CaseManagement.Domain.Abstractions.Entities;

public class BaseEntitySchema : BaseEntityTemplate
{
    [DisplayName(TextConstants.LabelFormSchemaIdentifier)]
    [Description(TextConstants.HelpFormSchemaIdentifier)]
    public required string SchemaIdentifier { get; set; }
    
    [JsonIgnore]
    public EntitySchema? Schema { get; set; }
    public required Guid SchemaID { get; set; }
}
