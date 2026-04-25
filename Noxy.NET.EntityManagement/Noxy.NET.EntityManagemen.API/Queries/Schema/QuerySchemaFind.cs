using MediatR;
using Noxy.NET.EntityManagement.Domain.Responses.Schema;

namespace Noxy.NET.EntityManagement.API.Queries.Schema;

public class QuerySchemaFind(Guid id) : IRequest<ResponseSchemaFind>
{
    public Guid ID { get; } = id;
}
