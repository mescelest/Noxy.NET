using MediatR;
using Noxy.NET.EntityManagement.Domain.Requests.Schema;
using Noxy.NET.EntityManagement.Domain.Responses.Schema;

namespace Noxy.NET.EntityManagement.API.Queries.Schema;

public class QuerySchemaContextList(RequestSchemaContextList request) : IRequest<ResponseSchemaContextList>
{
    public Guid? SchemaID { get; } = request.SchemaID;
    public string? Search { get; } = request.Search;
    public int? PageNumber { get; } = request.PageNumber;
    public int? PageSize { get; } = request.PageSize;
}
