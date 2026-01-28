using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Noxy.NET.CaseManagement.Domain.Abstractions.Forms;
using Noxy.NET.CaseManagement.Domain.Constants;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas;

namespace Noxy.NET.CaseManagement.Domain.Forms.Schemas.Forms;

public class FormModelSchemaPropertyInteger(EntitySchemaPropertyInteger? entity) : BaseFormModelEntitySchemaComponent(entity)
{
    public override string APIEndpoint => "Schema/Property/Integer";
    public override HttpMethod HttpMethod => HttpMethod.Post;

    [Required]
    [DisplayName(TextConstants.LabelFormIsUnsigned)]
    [Description(TextConstants.HelpFormIsUnsigned)]
    public bool IsUnsigned { get; set; } = entity?.IsUnsigned ?? false;

    [Required]
    [DisplayName(TextConstants.LabelFormDefaultValue)]
    [Description(TextConstants.HelpFormDefaultValue)]
    public Guid DefaultValueDynamicID { get; set; } = entity?.DefaultValueDynamicID ?? Guid.NewGuid();

    [Required]
    [DisplayName(TextConstants.LabelFormMin)]
    [Description(TextConstants.HelpFormMin)]
    public Guid MinDynamicID { get; set; } = entity?.DefaultValueDynamicID ?? Guid.NewGuid();

    [Required]
    [DisplayName(TextConstants.LabelFormMax)]
    [Description(TextConstants.HelpFormMax)]
    public Guid MaxDynamicID { get; set; } = entity?.DefaultValueDynamicID ?? Guid.NewGuid();

    [JsonConstructor]
    public FormModelSchemaPropertyInteger() : this(null)
    {
    }
}
