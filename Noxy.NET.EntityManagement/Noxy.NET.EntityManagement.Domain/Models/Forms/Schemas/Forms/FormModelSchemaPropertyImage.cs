using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Noxy.NET.EntityManagement.Domain.Abstractions.Forms;
using Noxy.NET.EntityManagement.Domain.Constants;
using Noxy.NET.EntityManagement.Domain.Entities.Schemas;

namespace Noxy.NET.EntityManagement.Domain.Models.Forms.Schemas.Forms;

public class FormModelSchemaPropertyImage(EntitySchemaPropertyImage? entity) : BaseFormModelEntitySchemaComponent(entity)
{
    // [Required]
    // [DisplayName(TextConstants.LabelFormDefaultValue)]
    // [Description(TextConstants.HelpFormDefaultValue)]
    // public Guid DefaultValueDynamicID { get; set; } = entity?.DefaultValueDynamicID ?? Guid.NewGuid();
    //
    // [Required]
    // [DisplayName(TextConstants.LabelFormWidth)]
    // [Description(TextConstants.HelpFormWidth)]
    // public Guid WidthDynamicID { get; set; } = entity?.DefaultValueDynamicID ?? Guid.NewGuid();
    //
    // [Required]
    // [DisplayName(TextConstants.LabelFormHeight)]
    // [Description(TextConstants.HelpFormHeight)]
    // public Guid HeightDynamicID { get; set; } = entity?.DefaultValueDynamicID ?? Guid.NewGuid();

    [JsonConstructor]
    public FormModelSchemaPropertyImage() : this(null)
    {
    }

    public override string APIEndpoint => "Schema/Property/Image";
    public override HttpMethod HttpMethod => HttpMethod.Post;

    [Required]
    [DisplayName(TextConstants.LabelFormAllowedExtensions)]
    [Description(TextConstants.HelpFormAllowedExtensions)]
    public string AllowedExtensions { get; set; } = entity?.AllowedExtensions ?? string.Empty;
}
