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
    [DisplayName(TextConstants.LabelFormSchemaIdentifier)]
    [Description(TextConstants.HelpFormSchemaIdentifier)]
    public string SchemaIdentifier { get; set; } = string.Empty;

    [Required]
    [DisplayName(TextConstants.LabelFormName)]
    [Description(TextConstants.HelpFormName)]
    public string Name { get; set; } = string.Empty;

    [DisplayName(TextConstants.LabelFormNote)]
    [Description(TextConstants.HelpFormNote)]
    public string Note { get; set; } = string.Empty;

    [Required]
    [DisplayName(TextConstants.LabelFormWeight)]
    [Description(TextConstants.HelpFormWeight)]
    public int? Weight { get; set; } = 0;

    [Required]
    [DisplayName(TextConstants.LabelFormDateTimeType)]
    [Description(TextConstants.HelpFormDateTimeType)]
    public DateTimeTypeEnum Type { get; set; }

    [NotEmptyGuid]
    [DisplayName(TextConstants.LabelFormTitle)]
    [Description(TextConstants.HelpFormTitle)]
    public Guid TitleParameterTextID { get; set; }

    [DisplayName(TextConstants.LabelFormDescription)]
    [Description(TextConstants.HelpFormDescription)]
    public Guid? DescriptionParameterTextID { get; set; }
}
