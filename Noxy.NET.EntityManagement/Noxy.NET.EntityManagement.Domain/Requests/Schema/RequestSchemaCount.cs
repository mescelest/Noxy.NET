using System.ComponentModel;
using Noxy.NET.EntityManagement.Domain.Abstractions.Requests;
using Noxy.NET.EntityManagement.Domain.Constants;
using Noxy.NET.EntityManagement.Domain.Responses.Schema;

namespace Noxy.NET.EntityManagement.Domain.Requests.Schema;

public class RequestSchemaCount : BaseRequestGet<ResponseSchemaCount>
{
    public override string APIEndpoint => "Schema/Count";

    [DisplayName(ParameterTextConstants.LabelFormSearch)]
    [Description(ParameterTextConstants.HelpFormSearch)]
    public string? Search { get; set; }

    [DisplayName(ParameterTextConstants.LabelFormIsActivated)]
    [Description(ParameterTextConstants.HelpFormIsActivated)]
    public bool? IsActivated { get; set; }

    public override Dictionary<string, object?> ToQueryParameters()
    {
        return new()
        {
            [nameof(Search)] = Search,
            [nameof(IsActivated)] = IsActivated,
        };
    }
}
