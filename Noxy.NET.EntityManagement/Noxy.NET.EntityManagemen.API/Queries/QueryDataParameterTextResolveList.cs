using MediatR;
using Noxy.NET.EntityManagement.Domain.Requests;
using Noxy.NET.EntityManagement.Domain.Responses;

namespace Noxy.NET.EntityManagement.API.Queries;

public class QueryDataParameterTextResolveList(RequestDataParameterTextResolveList request) : IRequest<ResponseDataParameterResolveList>
{
    public IEnumerable<string> SchemaIdentifierList { get; } = request.SchemaIdentifierList;
}
