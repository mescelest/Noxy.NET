using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Noxy.NET.EntityManagement.Domain.Abstractions.Requests;
using Noxy.NET.EntityManagement.Domain.Attributes;
using Noxy.NET.EntityManagement.Domain.Constants;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Property;

namespace Noxy.NET.EntityManagement.Domain.Requests.Schema.Property;

public class RequestSchemaPropertyImageUpdate : BaseRequestPost<ResponseSchemaPropertyImageUpdate>
{
    public override string APIEndpoint => $"schema/property/image/{ID}";

    [Required]
    public required Guid ID { get; init; }

    [Required]
    [IdentifierValidation]
    [DisplayName(ParameterTextConstants.LabelFormSchemaIdentifier)]
    [Description(ParameterTextConstants.HelpFormSchemaIdentifier)]
    public string SchemaIdentifier { get; set; } = string.Empty;

    [Required]
    [DisplayName(ParameterTextConstants.LabelFormName)]
    [Description(ParameterTextConstants.HelpFormName)]
    public string Name { get; set; } = string.Empty;

    [DisplayName(ParameterTextConstants.LabelFormNote)]
    [Description(ParameterTextConstants.HelpFormNote)]
    public string Note { get; set; } = string.Empty;

    [Required]
    [DisplayName(ParameterTextConstants.LabelFormWeight)]
    [Description(ParameterTextConstants.HelpFormWeight)]
    public int? Weight { get; set; } = 0;

    [DisplayName(ParameterTextConstants.LabelFormAllowedExtensions)]
    [Description(ParameterTextConstants.HelpFormAllowedExtensions)]
    public string AllowedExtensions { get; set; } = string.Empty;

    [NotEmptyGuid]
    [DisplayName(ParameterTextConstants.LabelFormTitle)]
    [Description(ParameterTextConstants.HelpFormTitle)]
    public Guid TitleParameterTextID { get; set; }

    [DisplayName(ParameterTextConstants.LabelFormDescription)]
    [Description(ParameterTextConstants.HelpFormDescription)]
    public Guid? DescriptionParameterTextID { get; set; }
}
