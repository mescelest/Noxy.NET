using System.Text.Json.Serialization;
using Noxy.NET.EntityManagement.Domain.Abstractions.Forms;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;

namespace Noxy.NET.EntityManagement.Domain.Models.Forms.Schemas.Forms;

public class FormModelSchemaPropertyDecimal(EntitySchemaPropertyDecimal? entity) : BaseFormModelEntitySchemaComponent(entity)
{
    // [Required]
    // [DisplayName(TextConstants.LabelFormDefaultValue)]
    // [Description(TextConstants.HelpFormDefaultValue)]
    // public Guid DefaultValueDynamicID { get; set; } = entity?.DefaultValueDynamicID ?? Guid.NewGuid();
    //
    // [Required]
    // [DisplayName(TextConstants.LabelFormMin)]
    // [Description(TextConstants.HelpFormMin)]
    // public Guid MinDynamicID { get; set; } = entity?.DefaultValueDynamicID ?? Guid.NewGuid();
    //
    // [Required]
    // [DisplayName(TextConstants.LabelFormMax)]
    // [Description(TextConstants.HelpFormMax)]
    // public Guid MaxDynamicID { get; set; } = entity?.DefaultValueDynamicID ?? Guid.NewGuid();

    [JsonConstructor]
    public FormModelSchemaPropertyDecimal() : this(null)
    {
    }

    public override string APIEndpoint => "Schema/Property/Decimal";
    public override HttpMethod HttpMethod => HttpMethod.Post;
}
