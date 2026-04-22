using MediatR;
using Noxy.NET.EntityManagement.Domain.Responses.Schema;

namespace Noxy.NET.EntityManagement.API.Commands.Schema;

public class CommandSchemaDelete(Guid id) : IRequest<ResponseSchemaDelete>
{
    public Guid ID { get; } = id;
}
