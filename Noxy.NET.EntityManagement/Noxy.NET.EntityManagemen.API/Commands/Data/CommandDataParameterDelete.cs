using Mediator;
using Noxy.NET.EntityManagement.Domain.Responses.Data;

namespace Noxy.NET.EntityManagement.API.Commands.Data;

public class CommandDataParameterDelete(Guid id) : ICommand<ResponseDataParameterDelete>
{
    public Guid ID { get; set; } = id;
}
