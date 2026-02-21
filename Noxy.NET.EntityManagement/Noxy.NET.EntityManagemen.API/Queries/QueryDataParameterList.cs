using MediatR;
using Noxy.NET.EntityManagement.Domain.Responses.Data;

namespace Noxy.NET.EntityManagement.API.Queries;

public class QueryDataParameterList(string identifier) : IRequest<ResponseDataParameterList>
{
    public string SchemaIdentifier { get; } = identifier;
}
