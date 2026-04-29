using MediatR;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Context;

namespace Noxy.NET.EntityManagement.API.Queries.Schema.Context;

public class QuerySchemaContextFind(Guid id) : IRequest<ResponseSchemaContextFind>
{
    public Guid ID { get; } = id;
}
