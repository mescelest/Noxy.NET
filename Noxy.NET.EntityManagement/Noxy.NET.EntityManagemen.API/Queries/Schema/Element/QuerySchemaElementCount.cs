using MediatR;
using Noxy.NET.EntityManagement.Domain.Requests.Schema.Element;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Element;

namespace Noxy.NET.EntityManagement.API.Queries.Schema.Element;

public class QuerySchemaElementCount(RequestSchemaElementCount request) : IRequest<ResponseSchemaElementCount>
{
    public Guid? SchemaID { get; } = request.SchemaID;
    public string? Search { get; } = request.Search;
}
