using Mediator;
using Noxy.NET.EntityManagement.Domain.Responses.Schema.Parameter;

namespace Noxy.NET.EntityManagement.API.Commands.Schema.Parameter;

public class CommandSchemaParameterDelete(Guid id) : ICommand<ResponseSchemaParameterDelete>
{
    public Guid ID { get; } = id;
}
