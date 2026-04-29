using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Noxy.NET.EntityManagement.Domain.Abstractions.Requests;
using Noxy.NET.EntityManagement.Domain.Attributes;
using Noxy.NET.EntityManagement.Domain.Constants;
using Noxy.NET.EntityManagement.Domain.Enums;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Parameter;

namespace Noxy.NET.EntityManagement.Domain.Requests.Schema.Parameter;

public class RequestSchemaParameterTextCreate : BaseRequestPost<ResponseSchemaParameterTextCreate>
{
    public override string APIEndpoint => "Schema/Parameter/Text";

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
    [DisplayName(TextConstants.LabelFormParameterTextType)]
    [Description(TextConstants.HelpFormParameterTextType)]
    public ParameterTextTypeEnum Type { get; set; } = ParameterTextTypeEnum.Line;

    [Required]
    [DisplayName(TextConstants.LabelFormIsSystemDefined)]
    [Description(TextConstants.HelpFormIsSystemDefined)]
    public bool IsSystemDefined { get; set; }

    [Required]
    [DisplayName(TextConstants.LabelFormIsApprovalRequired)]
    [Description(TextConstants.HelpFormIsApprovalRequired)]
    public bool IsApprovalRequired { get; set; }
}
