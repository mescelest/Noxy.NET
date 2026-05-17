using Mediator;
using Noxy.NET.EntityManagement.Domain.Responses.Schema;

namespace Noxy.NET.EntityManagement.API.Commands.Schema;

public class CommandSchemaActivate(Guid id) : ICommand<ResponseSchemaActivate>
{
    public Guid ID { get; } = id;
}
