using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Noxy.NET.CaseManagement.Domain.Abstractions.Forms;
using Noxy.NET.CaseManagement.Domain.Constants;
using Noxy.NET.CaseManagement.Domain.Entities.Schemas;
using Noxy.NET.CaseManagement.Domain.Enums;

namespace Noxy.NET.CaseManagement.Domain.Forms.Schemas.Forms;

public class FormModelSchemaPropertyDateTime(EntitySchemaPropertyDateTime? entity) : BaseFormModelEntitySchemaComponent(entity)
{
    public override string APIEndpoint => "Schema/Property/DateTime";
    public override HttpMethod HttpMethod => HttpMethod.Post;

    [Required]
    public DateTimeTypeEnum Type { get; set; } = entity?.Type ?? DateTimeTypeEnum.Date;
    
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
    public FormModelSchemaPropertyDateTime() : this(null)
    {
    }
}
