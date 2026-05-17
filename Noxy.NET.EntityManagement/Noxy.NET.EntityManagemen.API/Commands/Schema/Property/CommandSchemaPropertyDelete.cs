using Mediator;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Property;

namespace Noxy.NET.EntityManagement.API.Commands.Schema.Property;

public class CommandSchemaPropertyDelete(Guid id) : ICommand<ResponseSchemaPropertyDelete>
{
    public Guid ID { get; } = id;
}
