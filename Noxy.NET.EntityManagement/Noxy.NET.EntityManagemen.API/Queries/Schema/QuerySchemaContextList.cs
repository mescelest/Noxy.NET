using MediatR;
using Noxy.NET.EntityManagement.Domain.Requests.Schema;
using Noxy.NET.EntityManagement.Domain.Responses.Schema;

namespace Noxy.NET.EntityManagement.API.Queries.Schema;

public class QuerySchemaContextList(Guid schemaID, RequestSchemaContextList request) : IRequest<ResponseSchemaContextList>
{
    public Guid SchemaID { get; } = schemaID;
    public string? Search { get; } = request.Search;
    public int? PageNumber { get; } = request.PageNumber;
    public int? PageSize { get; } = request.PageSize;
}
