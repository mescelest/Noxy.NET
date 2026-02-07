using MediatR;
using Noxy.NET.EntityManagement.Domain.Requests;
using Noxy.NET.EntityManagement.Domain.Responses;

namespace Noxy.NET.EntityManagement.API.Queries;

public class QueryDataParameterTextResolveList(RequestDataParameterTextResolveList requestData) : IRequest<ResponseDataParameterResolveList>
{
    public IEnumerable<string> SchemaIdentifierList { get; } = requestData.SchemaIdentifierList;
}
