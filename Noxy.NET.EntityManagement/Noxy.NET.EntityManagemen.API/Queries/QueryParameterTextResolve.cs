using MediatR;

namespace Noxy.NET.EntityManagement.API.Queries;

public class QueryParameterTextResolve(string schemaIdentifier) : IRequest<string>
{
    public string SchemaIdentifier { get; } = schemaIdentifier;
}
