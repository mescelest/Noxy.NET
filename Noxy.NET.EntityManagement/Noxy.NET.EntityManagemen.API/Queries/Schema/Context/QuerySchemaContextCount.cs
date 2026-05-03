using MediatR;
using Noxy.NET.EntityManagement.Domain.Requests.Schema.Context;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Context;

namespace Noxy.NET.EntityManagement.API.Queries.Schema.Context;

public class QuerySchemaContextCount(RequestSchemaContextCount request) : IRequest<ResponseSchemaContextCount>
{
    public Guid? SchemaID { get; } = request.SchemaID;
    public string? Search { get; } = request.Search;
}
