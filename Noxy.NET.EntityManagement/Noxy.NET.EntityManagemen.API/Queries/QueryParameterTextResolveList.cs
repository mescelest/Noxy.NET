using MediatR;
using Noxy.NET.EntityManagement.Domain.Requests;

namespace Noxy.NET.EntityManagement.API.Queries;

public class QueryParameterTextResolveList(RequestDataParameterTextResolveList requestData) : IRequest<Dictionary<string, string>>
{
    public IEnumerable<string> SchemaIdentifierList { get; } = requestData.SchemaIdentifierList;
}
