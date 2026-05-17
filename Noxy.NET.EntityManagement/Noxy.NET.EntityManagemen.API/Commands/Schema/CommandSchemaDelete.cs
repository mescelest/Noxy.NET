using Mediator;
using Noxy.NET.EntityManagement.Domain.Responses.Schema;

namespace Noxy.NET.EntityManagement.API.Commands.Schema;

public class CommandSchemaDelete(Guid id) : ICommand<ResponseSchemaDelete>
{
    public Guid ID { get; } = id;
}
