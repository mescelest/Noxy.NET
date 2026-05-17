using Mediator;
using Noxy.NET.EntityManagement.Domain.Responses.Schema;

namespace Noxy.NET.EntityManagement.API.Queries.Schema;

public class QuerySchemaFind(Guid id) : IQuery<ResponseSchemaFind>
{
    public Guid ID { get; } = id;
}
