using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Noxy.NET.EntityManagement.Domain.Abstractions.Requests;
using Noxy.NET.EntityManagement.Domain.Attributes;
using Noxy.NET.EntityManagement.Domain.Constants;
using Noxy.NET.EntityManagement.Domain.Enums;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Property;

namespace Noxy.NET.EntityManagement.Domain.Requests.Schema.Property;

public class RequestSchemaPropertyDateTimeCreate : BaseRequestPost<ResponseSchemaPropertyDateTimeCreate>
{
    public override string APIEndpoint => "Schema/Property/DateTime";

    public Guid? SchemaID { get; set; }

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

    [Required]
    [DisplayName(ParameterTextConstants.LabelFormDateTimeType)]
    [Description(ParameterTextConstants.HelpFormDateTimeType)]
    public DateTimeTypeEnum Type { get; set; }

    [NotEmptyGuid]
    [DisplayName(ParameterTextConstants.LabelFormTitle)]
    [Description(ParameterTextConstants.HelpFormTitle)]
    public Guid TitleParameterTextID { get; set; }

    [DisplayName(ParameterTextConstants.LabelFormDescription)]
    [Description(ParameterTextConstants.HelpFormDescription)]
    public Guid? DescriptionParameterTextID { get; set; }
}
