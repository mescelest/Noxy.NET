using System.ComponentModel;
using Noxy.NET.EntityManagement.Domain.Abstractions.Requests;
using Noxy.NET.EntityManagement.Domain.Constants;
using Noxy.NET.EntityManagement.Domain.Responses.Schema;

namespace Noxy.NET.EntityManagement.Domain.Requests.Schema;

public class RequestSchemaList : BaseRequestGetList<ResponseSchemaList>
{
    public override string APIEndpoint => "schema";

    [DisplayName(ParameterTextConstants.LabelFormIsActivated)]
    [Description(ParameterTextConstants.HelpFormIsActivated)]
    public bool? IsActivated { get; set; }

    public override Dictionary<string, object?> ToQueryParameters()
    {
        return new()
        {
            [nameof(Search)] = Search,
            [nameof(PageSize)] = PageSize,
            [nameof(PageNumber)] = PageNumber,
            [nameof(SortColumn)] = SortColumn,
            [nameof(SortDirection)] = SortDirection,
            [nameof(IsActivated)] = IsActivated,
        };
    }
}
