using MediatR;
using Noxy.NET.EntityManagement.Domain.Requests.Schema;
using Noxy.NET.EntityManagement.Domain.Responses.Schema;

namespace Noxy.NET.EntityManagement.API.Queries;

public class QuerySchemaList(RequestSchemaList request) : IRequest<ResponseSchemaList>
{
    public string? Search { get; } = request.Search;
    public int? PageNumber { get; } = request.PageNumber;
    public int? PageSize { get; } = request.PageSize;
}
