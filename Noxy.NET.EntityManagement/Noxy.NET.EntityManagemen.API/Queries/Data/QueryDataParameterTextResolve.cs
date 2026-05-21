using Mediator;
using Noxy.NET.EntityManagement.Domain.Responses.Data.Parameter;

namespace Noxy.NET.EntityManagement.API.Queries.Data;

public class QueryDataParameterTextResolve(string schemaIdentifier) : IQuery<ResponseDataParameterTextResolve>
{
    public string SchemaIdentifier { get; } = schemaIdentifier;
}
