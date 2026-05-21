using Mediator;
using Noxy.NET.EntityManagement.Domain.Responses.Data.Parameter;

namespace Noxy.NET.EntityManagement.API.Queries.Data;

public class QueryDataParameterFind(Guid id) : IQuery<ResponseDataParameterFind>
{
    public Guid ID { get; } = id;
}
