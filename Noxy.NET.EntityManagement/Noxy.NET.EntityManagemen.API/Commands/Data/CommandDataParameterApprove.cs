using Mediator;
using Noxy.NET.EntityManagement.Domain.Responses.Data.Parameter;

namespace Noxy.NET.EntityManagement.API.Commands.Data;

public class CommandDataParameterApprove(Guid id) : ICommand<ResponseDataParameterApprove>
{
    public Guid ID { get; set; } = id;
}
