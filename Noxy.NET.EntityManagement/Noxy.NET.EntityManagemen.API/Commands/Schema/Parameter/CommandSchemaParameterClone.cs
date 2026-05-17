using Mediator;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Parameter;

namespace Noxy.NET.EntityManagement.API.Commands.Schema.Parameter;

public class CommandSchemaParameterClone(Guid id) : ICommand<ResponseSchemaParameterClone>
{
    public Guid ID { get; } = id;
}
