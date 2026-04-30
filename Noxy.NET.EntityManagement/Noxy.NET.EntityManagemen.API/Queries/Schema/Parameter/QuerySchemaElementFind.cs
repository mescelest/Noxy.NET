using MediatR;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Parameter;

namespace Noxy.NET.EntityManagement.API.Queries.Schema.Parameter;

public class QuerySchemaParameterFind(Guid id) : IRequest<ResponseSchemaParameterFind>
{
    public Guid ID { get; } = id;
}
