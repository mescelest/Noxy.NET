using Mediator;
using Noxy.NET.EntityManagement.Domain.Requests.Data.Parameter;
using Noxy.NET.EntityManagement.Domain.Responses.Data.Parameter;

namespace Noxy.NET.EntityManagement.API.Queries.Data;

public class QueryDataParameterSystemResolveList(RequestDataParameterSystemResolveList request) : IQuery<ResponseDataParameterSystemResolveList>
{
    public IEnumerable<string> SchemaIdentifierList { get; } = request.SchemaIdentifierList;
}
