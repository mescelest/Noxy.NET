using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Noxy.NET.EntityManagement.Domain.Abstractions.Requests;
using Noxy.NET.EntityManagement.Domain.Constants;
using Noxy.NET.EntityManagement.Domain.Responses.Data.Parameter;

namespace Noxy.NET.EntityManagement.Domain.Requests.Data.Parameter;

public class RequestDataParameterTextCreate : BaseRequestPost<ResponseDataParameterTextCreate>
{
    public override string APIEndpoint => "/data/parameter/text";

    [Required]
    [DisplayName(ParameterTextConstants.LabelFormSchemaIdentifier)]
    [Description(ParameterTextConstants.HelpFormSchemaIdentifier)]
    public required string SchemaIdentifier { get; set; }

    [Required]
    [DisplayName(ParameterTextConstants.LabelFormCulture)]
    [Description(ParameterTextConstants.HelpFormCulture)]
    public required string Culture { get; set; }

    [Required]
    [DisplayName(ParameterTextConstants.LabelFormValue)]
    [Description(ParameterTextConstants.HelpFormValue)]
    public string Value { get; set; } = string.Empty;

    [DisplayName(ParameterTextConstants.LabelFormDateEffective)]
    [Description(ParameterTextConstants.HelpFormDateEffective)]
    public DateTime? TimeEffective { get; set; }
}
