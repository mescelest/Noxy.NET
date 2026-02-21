using MediatR;
using Noxy.NET.EntityManagement.Domain.Requests.Data;
using Noxy.NET.EntityManagement.Domain.Responses.Data;

namespace Noxy.NET.EntityManagement.API.Queries;

public class QueryDataParameterTextResolveList(RequestDataParameterTextResolveList request) : IRequest<ResponseDataParameterResolveList>
{
    public IEnumerable<string> SchemaIdentifierList { get; } = request.SchemaIdentifierList;
}
