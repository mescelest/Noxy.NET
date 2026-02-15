using MediatR;
using Noxy.NET.EntityManagement.Domain.Responses;

namespace Noxy.NET.EntityManagement.API.Commands;

public class CommandDataParameterDelete(Guid id) : IRequest<ResponseDataParameterDelete>
{
    public Guid ID { get; set; } = id;
}
