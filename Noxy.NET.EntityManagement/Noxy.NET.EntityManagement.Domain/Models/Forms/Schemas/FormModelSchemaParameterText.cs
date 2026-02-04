using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Noxy.NET.EntityManagement.Domain.Abstractions.Forms;
using Noxy.NET.EntityManagement.Domain.Constants;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Enums;

namespace Noxy.NET.EntityManagement.Domain.Models.Forms.Schemas;

public class FormModelSchemaParameterText(EntitySchemaParameterText? entity = null) : BaseFormModelEntitySchemaParameter(entity)
{
    [JsonConstructor]
    public FormModelSchemaParameterText() : this(null)
    {
    }

    public override string APIEndpoint => "Schema/Parameter/Code";
    public override HttpMethod HttpMethod => HttpMethod.Post;

    [Required]
    [DisplayName(TextConstants.LabelFormParameterTextType)]
    [Description(TextConstants.HelpFormParameterTextType)]
    public TextParameterTypeEnum Type { get; set; } = entity?.Type ?? TextParameterTypeEnum.Line;
}
