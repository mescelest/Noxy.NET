using Mediator;
using Noxy.NET.EntityManagement.Domain.Requests.Data.Parameter;
using Noxy.NET.EntityManagement.Domain.Responses.Data.Parameter;

namespace Noxy.NET.EntityManagement.API.Queries.Data;

public class QueryDataParameterCount(string identifier, RequestDataParameterCount request) : IQuery<ResponseDataParameterCount>
{
    public string SchemaIdentifier { get; } = identifier;
    public string? Search { get; } = request.Search;
}
