using Noxy.NET.EntityManagement.Domain.Abstractions.Requests;
using Noxy.NET.EntityManagement.Domain.Responses.Data.Parameter;

namespace Noxy.NET.EntityManagement.Domain.Requests.Data.Parameter;

public class RequestDataParameterList : BaseRequestGetList<ResponseDataParameterList>
{
    public override string APIEndpoint => $"/data/parameter/by-identifier/{SchemaIdentifier}";

    public required string? SchemaIdentifier { get; init; }

    public override Dictionary<string, object?> ToQueryParameters()
    {
        return new()
        {
            [nameof(Search)] = Search,
            [nameof(PageSize)] = PageSize,
            [nameof(PageNumber)] = PageNumber,
            [nameof(SortColumn)] = SortColumn,
            [nameof(SortDirection)] = SortDirection,
        };
    }
}
