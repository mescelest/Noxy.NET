using MediatR;
using Noxy.NET.EntityManagement.Domain.Responses;

namespace Noxy.NET.EntityManagement.API.Queries;

public class QueryDataParameterTextResolve(string schemaIdentifier) : IRequest<ResponseDataParameterResolve>
{
    public string SchemaIdentifier { get; } = schemaIdentifier;
}
