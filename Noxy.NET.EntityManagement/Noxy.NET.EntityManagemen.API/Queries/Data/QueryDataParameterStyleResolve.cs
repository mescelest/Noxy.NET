using Mediator;
using Noxy.NET.EntityManagement.Domain.Responses.Data.Parameter;

namespace Noxy.NET.EntityManagement.API.Queries.Data;

public class QueryDataParameterStyleResolve(string schemaIdentifier) : IQuery<ResponseDataParameterStyleResolve>
{
    public string SchemaIdentifier { get; } = schemaIdentifier;
}
