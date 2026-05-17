using Mediator;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Context;

namespace Noxy.NET.EntityManagement.API.Commands.Schema.Context;

public class CommandSchemaContextClone(Guid id) : ICommand<ResponseSchemaContextClone>
{
    public Guid ID { get; } = id;
}
