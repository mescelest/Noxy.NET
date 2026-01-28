using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Noxy.NET.EntityManagement.Domain.Abstractions.Forms;
using Noxy.NET.EntityManagement.Domain.Constants;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;
using Noxy.NET.EntityManagement.Domain.Enums;

namespace Noxy.NET.EntityManagement.Domain.Models.Forms.Schemas.Forms;

public class FormModelSchemaDynamicValueTextParameter(EntitySchemaDynamicValueTextParameter? entity = null) : BaseFormModelEntitySchemaDynamicValueParameter(entity)
{
    [JsonConstructor]
    public FormModelSchemaDynamicValueTextParameter() : this(null)
    {
    }

    public override string APIEndpoint => "Schema/DynamicValue/Code";
    public override HttpMethod HttpMethod => HttpMethod.Post;

    [Required]
    [DisplayName(TextConstants.LabelFormTextParameterType)]
    [Description(TextConstants.HelpFormTextParameterType)]
    public TextParameterTypeEnum Type { get; set; } = entity?.Type ?? TextParameterTypeEnum.Line;
}
