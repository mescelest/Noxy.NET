using System.ComponentModel;
using Mediator;
using Noxy.NET.EntityManagement.Domain.Requests.Schema;
using Noxy.NET.EntityManagement.Domain.Responses.Schema;

namespace Noxy.NET.EntityManagement.API.Queries.Schema;

public class QuerySchemaList(RequestSchemaList request) : IQuery<ResponseSchemaList>
{
    public bool? IsActivated { get; } = request.IsActivated;
    public string? Search { get; } = request.Search;
    public int? PageNumber { get; } = request.PageNumber;
    public int? PageSize { get; } = request.PageSize;
    public string? SortColumn { get; set; } = request.SortColumn;
    public ListSortDirection? SortDirection { get; set; } = request.SortDirection;
}
