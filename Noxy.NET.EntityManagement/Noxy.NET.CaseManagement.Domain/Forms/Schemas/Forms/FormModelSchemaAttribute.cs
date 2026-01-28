using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Noxy.NET.CaseManagement.Domain.Abstractions.Forms;
using Noxy.NET.CaseManagement.Domain.Constants;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas;
using Noxy.NET.CaseManagement.Domain.Enums;

namespace Noxy.NET.CaseManagement.Domain.Forms.Schemas.Forms;

public class FormModelSchemaAttribute(EntitySchemaAttribute? entity) : BaseFormModelEntitySchema(entity)
{
    public override string APIEndpoint => "Schema/Attribute";
    public override HttpMethod HttpMethod => HttpMethod.Post;

    [Required]
    [DisplayName(TextConstants.LabelFormAttributeType)]
    [Description(TextConstants.HelpFormAttributeType)]
    public AttributeTypeEnum Type { get; set; } = entity?.Type ?? AttributeTypeEnum.String;

    [Required]
    [DisplayName(TextConstants.LabelFormIsValueList)]
    [Description(TextConstants.HelpFormIsValueList)]
    public bool IsValueList { get; set; } = entity?.IsValueList ?? false;

    [JsonConstructor]
    public FormModelSchemaAttribute() : this(null)
    {
    }
}
