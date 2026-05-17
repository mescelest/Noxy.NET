using Mediator;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.ElementHasProperty;

namespace Noxy.NET.EntityManagement.API.Queries.Schema.ElementHasProperty;

public class QuerySchemaElementHasPropertyFind(Guid id) : IQuery<ResponseSchemaElementHasPropertyFind>
{
    public Guid ID { get; } = id;
}
