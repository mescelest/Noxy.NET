using Noxy.NET.EntityManagement.Domain.Abstractions.Requests;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Element;

namespace Noxy.NET.EntityManagement.Domain.Requests.Schema.Element;

public class RequestSchemaElementList : BaseRequestGetList<ResponseSchemaElementList>
{
    public override string APIEndpoint => "/schema/element";

    public Guid? SchemaID { get; init; }

    public override Dictionary<string, object?> ToQueryParameters()
    {
        return new()
        {
            [nameof(SchemaID)] = SchemaID,
            [nameof(Search)] = Search,
            [nameof(PageSize)] = PageSize,
            [nameof(PageNumber)] = PageNumber,
            [nameof(SortColumn)] = SortColumn,
            [nameof(SortDirection)] = SortDirection,
        };
    }
}
