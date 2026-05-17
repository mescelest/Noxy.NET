using Mediator;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Parameter;

namespace Noxy.NET.EntityManagement.API.Queries.Schema.Parameter;

public class QuerySchemaParameterFind(Guid id) : IQuery<ResponseSchemaParameterFind>
{
    public Guid ID { get; } = id;
}
