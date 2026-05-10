using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Noxy.NET.EntityManagement.Domain.Abstractions.Requests;
using Noxy.NET.EntityManagement.Domain.Constants;
using Noxy.NET.EntityManagement.Domain.Responses.Data;

namespace Noxy.NET.EntityManagement.Domain.Requests.Data;

public class RequestDataParameterSystemCreate : BaseRequestPost<ResponseDataParameterSystemCreate>
{
    public override string APIEndpoint => "/Data/Parameter/System";

    [Required]
    [DisplayName(ParameterTextConstants.LabelFormSchemaIdentifier)]
    [Description(ParameterTextConstants.HelpFormSchemaIdentifier)]
    public required string SchemaIdentifier { get; set; }

    [Required]
    [DisplayName(ParameterTextConstants.LabelFormValue)]
    [Description(ParameterTextConstants.HelpFormValue)]
    public string Value { get; set; } = string.Empty;

    [DisplayName(ParameterTextConstants.LabelFormDateEffective)]
    [Description(ParameterTextConstants.HelpFormDateEffective)]
    public DateTime? TimeEffective { get; set; }
}
