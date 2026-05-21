using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Noxy.NET.EntityManagement.Domain.Abstractions.Requests;
using Noxy.NET.EntityManagement.Domain.Attributes;
using Noxy.NET.EntityManagement.Domain.Constants;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Parameter;

namespace Noxy.NET.EntityManagement.Domain.Requests.Schema.Parameter;

public class RequestSchemaParameterStyleCreate : BaseRequestPost<ResponseSchemaParameterStyleCreate>
{
    public override string APIEndpoint => "schema/parameter/style";

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
    [DisplayName(ParameterTextConstants.LabelFormIsSystemDefined)]
    [Description(ParameterTextConstants.HelpFormIsSystemDefined)]
    public bool IsSystemDefined { get; set; }

    [Required]
    [DisplayName(ParameterTextConstants.LabelFormIsApprovalRequired)]
    [Description(ParameterTextConstants.HelpFormIsApprovalRequired)]
    public bool IsApprovalRequired { get; set; }
}
