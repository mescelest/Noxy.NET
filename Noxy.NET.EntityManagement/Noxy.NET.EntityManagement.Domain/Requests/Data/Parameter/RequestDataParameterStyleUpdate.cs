using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Noxy.NET.EntityManagement.Domain.Abstractions.Requests;
using Noxy.NET.EntityManagement.Domain.Constants;
using Noxy.NET.EntityManagement.Domain.Responses.Data.Parameter;

namespace Noxy.NET.EntityManagement.Domain.Requests.Data.Parameter;

public class RequestDataParameterStyleUpdate : BaseRequestPost<ResponseDataParameterStyleUpdate>
{
    public override string APIEndpoint => $"/data/parameter/style/{ID}";

    [Required]
    public required Guid ID { get; init; }

    [Required]
    [DisplayName(ParameterTextConstants.LabelFormValue)]
    [Description(ParameterTextConstants.HelpFormValue)]
    public string Value { get; set; } = string.Empty;

    [DisplayName(ParameterTextConstants.LabelFormDateEffective)]
    [Description(ParameterTextConstants.HelpFormDateEffective)]
    public DateTime? TimeEffective { get; set; }
}
