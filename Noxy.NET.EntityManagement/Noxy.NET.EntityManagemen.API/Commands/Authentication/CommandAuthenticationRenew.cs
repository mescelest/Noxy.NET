using MediatR;
using Noxy.NET.EntityManagement.Domain.Responses.Authentication;

namespace Noxy.NET.EntityManagement.API.Commands.Authentication;

public class CommandAuthenticationRenew(string email) : IRequest<ResponseAuthenticationRenew>
{
    public string Email { get; } = email;
}
