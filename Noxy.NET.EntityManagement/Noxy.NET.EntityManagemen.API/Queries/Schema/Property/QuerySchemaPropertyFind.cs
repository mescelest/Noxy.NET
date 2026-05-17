using Mediator;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Property;

namespace Noxy.NET.EntityManagement.API.Queries.Schema.Property;

public class QuerySchemaPropertyFind(Guid id) : IQuery<ResponseSchemaPropertyFind>
{
    public Guid ID { get; } = id;
}
