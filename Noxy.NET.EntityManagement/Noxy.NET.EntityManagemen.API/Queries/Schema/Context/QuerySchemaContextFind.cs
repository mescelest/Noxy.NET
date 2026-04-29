using MediatR;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Element;

namespace Noxy.NET.EntityManagement.API.Queries.Schema.Element;

public class QuerySchemaContextFind(Guid id) : IRequest<ResponseSchemaContextFind>
{
    public Guid ID { get; } = id;
}
