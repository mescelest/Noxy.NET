using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Noxy.NET.EntityManagement.Domain.Abstractions.Requests;
using Noxy.NET.EntityManagement.Domain.Constants;
using Noxy.NET.EntityManagement.Domain.Responses;

namespace Noxy.NET.EntityManagement.Domain.Requests;

public class RequestDataParameterSystemCreate : BaseRequestGet<ResponseDataParameterSystemCreate>
{
    public override string APIEndpoint => "/Data/Parameter/System";

    [Required]
    [DisplayName(TextConstants.LabelFormSchemaIdentifier)]
    [Description(TextConstants.HelpFormSchemaIdentifier)]
    public required string SchemaIdentifier { get; set; }

    [Required]
    [DisplayName(TextConstants.LabelFormValue)]
    [Description(TextConstants.HelpFormValue)]
    public string Value { get; set; } = string.Empty;

    [DisplayName(TextConstants.LabelFormDateEffective)]
    [Description(TextConstants.HelpFormDateEffective)]
    public DateTime? DateEffective { get; set; }
}
