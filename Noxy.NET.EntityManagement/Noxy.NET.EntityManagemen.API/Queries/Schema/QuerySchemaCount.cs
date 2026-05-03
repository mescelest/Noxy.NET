using MediatR;
using Noxy.NET.EntityManagement.Domain.Requests.Schema;
using Noxy.NET.EntityManagement.Domain.Responses.Schema;

namespace Noxy.NET.EntityManagement.API.Queries.Schema;

public class QuerySchemaCount(RequestSchemaCount request) : IRequest<ResponseSchemaCount>
{
    public string? Search { get; } = request.Search;
}
