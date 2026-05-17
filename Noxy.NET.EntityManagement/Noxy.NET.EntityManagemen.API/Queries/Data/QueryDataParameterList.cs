using Mediator;
using Noxy.NET.EntityManagement.Domain.Responses.Data;

namespace Noxy.NET.EntityManagement.API.Queries.Data;

public class QueryDataParameterList(string identifier) : IQuery<ResponseDataParameterList>
{
    public string SchemaIdentifier { get; } = identifier;
}
