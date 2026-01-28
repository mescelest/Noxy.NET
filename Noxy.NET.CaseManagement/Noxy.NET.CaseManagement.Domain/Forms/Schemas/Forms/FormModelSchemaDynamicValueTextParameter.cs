using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Noxy.NET.CaseManagement.Domain.Abstractions.Forms;
using Noxy.NET.CaseManagement.Domain.Constants;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas;
using Noxy.NET.CaseManagement.Domain.Enums;

namespace Noxy.NET.CaseManagement.Domain.Forms.Schemas.Forms;

public class FormModelSchemaDynamicValueTextParameter(EntitySchemaDynamicValueTextParameter? entity = null) : BaseFormModelEntitySchemaDynamicValueParameter(entity)
{
    public override string APIEndpoint =>  "Schema/DynamicValue/Code";
    public override HttpMethod HttpMethod => HttpMethod.Post;

    [Required]
    [DisplayName(TextConstants.LabelFormTextParameterType)]
    [Description(TextConstants.HelpFormTextParameterType)]
    public TextParameterTypeEnum Type { get; set; } = entity?.Type ?? TextParameterTypeEnum.Line;
    
    [JsonConstructor]
    public FormModelSchemaDynamicValueTextParameter() : this(null)
    {
    }
}
