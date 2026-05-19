using Mediator;
using Noxy.NET.EntityManagement.Domain.Requests.Data;
using Noxy.NET.EntityManagement.Domain.Responses.Data;

namespace Noxy.NET.EntityManagement.API.Queries.Data;

public class QueryDataParameterStyleResolveList(RequestDataParameterStyleResolveList request) : IQuery<ResponseDataParameterStyleResolveList>
{
    public IEnumerable<string> SchemaIdentifierList { get; } = request.SchemaIdentifierList;
}
