using MediatR;
using Noxy.NET.EntityManagement.Domain.Responses;

namespace Noxy.NET.EntityManagement.API.Commands;

public class CommandAuthenticationRenew(string email) : IRequest<ResponseAuthenticationRenew>
{
    public string Email { get; } = email;
}
