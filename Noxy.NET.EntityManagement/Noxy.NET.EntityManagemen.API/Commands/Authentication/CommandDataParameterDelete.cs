using MediatR;
using Noxy.NET.EntityManagement.Domain.Responses.Data;

namespace Noxy.NET.EntityManagement.API.Commands.Authentication;

public class CommandDataParameterDelete(Guid id) : IRequest<ResponseDataParameterDelete>
{
    public Guid ID { get; set; } = id;
}
