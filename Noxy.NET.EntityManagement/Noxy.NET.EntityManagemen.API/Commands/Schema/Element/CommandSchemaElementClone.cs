using Mediator;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Element;

namespace Noxy.NET.EntityManagement.API.Commands.Schema.Element;

public class CommandSchemaElementClone(Guid id) : ICommand<ResponseSchemaElementClone>
{
    public Guid ID { get; } = id;
}
