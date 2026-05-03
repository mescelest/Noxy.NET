using MediatR;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.ElementHasProperty;

namespace Noxy.NET.EntityManagement.API.Commands.Schema.ElementHasProperty;

public class CommandSchemaElementHasPropertyDelete(Guid id) : IRequest<ResponseSchemaElementHasPropertyDelete>
{
    public Guid ID { get; } = id;
}
