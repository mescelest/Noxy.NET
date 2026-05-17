using Mediator;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Context;

namespace Noxy.NET.EntityManagement.API.Commands.Schema.Context;

public class CommandSchemaContextDelete(Guid id) : ICommand<ResponseSchemaContextDelete>
{
    public Guid ID { get; } = id;
}
