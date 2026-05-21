using Mediator;
using Noxy.NET.EntityManagement.Domain.Responses.Data.Parameter;

namespace Noxy.NET.EntityManagement.API.Queries.Data;

public class QueryDataParameterSystemResolve(string schemaIdentifier) : IQuery<ResponseDataParameterSystemResolve>
{
    public string SchemaIdentifier { get; } = schemaIdentifier;
}
