using Mediator;
using Noxy.NET.EntityManagement.Domain.Responses.Schema;

namespace Noxy.NET.EntityManagement.API.Commands.Schema;

public sealed class CommandSchemaClone(Guid id) : ICommand<ResponseSchemaClone>
{
    public Guid ID { get; } = id;
}
