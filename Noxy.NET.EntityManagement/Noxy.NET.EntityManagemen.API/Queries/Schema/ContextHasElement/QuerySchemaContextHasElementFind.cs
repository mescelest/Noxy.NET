using Mediator;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.ContextHasElement;

namespace Noxy.NET.EntityManagement.API.Queries.Schema.ContextHasElement;

public class QuerySchemaContextHasElementFind(Guid id) : IQuery<ResponseSchemaContextHasElementFind>
{
    public Guid ID { get; } = id;
}
