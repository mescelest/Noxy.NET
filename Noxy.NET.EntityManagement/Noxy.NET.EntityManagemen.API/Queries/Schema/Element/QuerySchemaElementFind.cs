using MediatR;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Element;

namespace Noxy.NET.EntityManagement.API.Queries.Schema.Element;

public class QuerySchemaElementFind(Guid id) : IRequest<ResponseSchemaElementFind>
{
    public Guid ID { get; } = id;
}
