using MediatR;
using Noxy.NET.EntityManagement.Domain.Responses.Schema;

namespace Noxy.NET.EntityManagement.API.Commands.Schema;

public class CommandSchemaActivate(Guid id) : IRequest<ResponseSchemaActivate>
{
    public Guid ID { get; } = id;
}
