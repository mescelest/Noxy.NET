using Noxy.NET.EntityManagement.Domain.Abstractions.Requests;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Context;

namespace Noxy.NET.EntityManagement.Domain.Requests.Schema.Context;

public class RequestSchemaContextList : BaseRequestGetList<ResponseSchemaContextList>
{
    public override string APIEndpoint => "schema/context";

    public Guid? SchemaID { get; set; }

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
