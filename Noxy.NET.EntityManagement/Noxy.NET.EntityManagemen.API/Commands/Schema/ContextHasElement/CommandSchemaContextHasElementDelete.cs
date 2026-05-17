using Mediator;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.ContextHasElement;

namespace Noxy.NET.EntityManagement.API.Commands.Schema.ContextHasElement;

public class CommandSchemaContextHasElementDelete(Guid id) : ICommand<ResponseSchemaContextHasElementDelete>
{
    public Guid ID { get; } = id;
}
