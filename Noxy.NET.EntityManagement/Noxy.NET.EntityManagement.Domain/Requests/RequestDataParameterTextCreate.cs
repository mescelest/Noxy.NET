using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Noxy.NET.EntityManagement.Domain.Abstractions.Requests;
using Noxy.NET.EntityManagement.Domain.Constants;
using Noxy.NET.EntityManagement.Domain.Responses;

namespace Noxy.NET.EntityManagement.Domain.Requests;

public class RequestDataParameterTextCreate : BaseRequestPost<ResponseDataParameterTextCreate>
{
    public override string APIEndpoint => "/Data/Parameter/Text";

    [Required]
    [DisplayName(TextConstants.LabelFormSchemaIdentifier)]
    [Description(TextConstants.HelpFormSchemaIdentifier)]
    public required string SchemaIdentifier { get; set; }

    [Required]
    [DisplayName(TextConstants.LabelFormCulture)]
    [Description(TextConstants.HelpFormCulture)]
    public required string Culture { get; set; }

    [Required]
    [DisplayName(TextConstants.LabelFormValue)]
    [Description(TextConstants.HelpFormValue)]
    public string Value { get; set; } = string.Empty;

    [DisplayName(TextConstants.LabelFormDateEffective)]
    [Description(TextConstants.HelpFormDateEffective)]
    public DateTime? TimeEffective { get; set; }
}
