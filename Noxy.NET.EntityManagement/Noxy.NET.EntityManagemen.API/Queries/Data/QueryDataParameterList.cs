using System.ComponentModel;
using Mediator;
using Noxy.NET.EntityManagement.Domain.Requests.Data.Parameter;
using Noxy.NET.EntityManagement.Domain.Responses.Data.Parameter;

namespace Noxy.NET.EntityManagement.API.Queries.Data;

public class QueryDataParameterList(string identifier, RequestDataParameterList request) : IQuery<ResponseDataParameterList>
{
    public string SchemaIdentifier { get; } = identifier;
    public string? Search { get; } = request.Search;
    public int? PageNumber { get; } = request.PageNumber;
    public int? PageSize { get; } = request.PageSize;
    public string? SortColumn { get; set; } = request.SortColumn;
    public ListSortDirection? SortDirection { get; set; } = request.SortDirection;
}
