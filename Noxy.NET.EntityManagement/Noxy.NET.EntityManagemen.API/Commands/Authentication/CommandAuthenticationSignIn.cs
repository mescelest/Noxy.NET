using MediatR;
using Noxy.NET.EntityManagement.Domain.Requests.Authentication;
using Noxy.NET.EntityManagement.Domain.Responses.Authentication;

namespace Noxy.NET.EntityManagement.API.Commands.Authentication;

public class CommandAuthenticationSignIn(RequestAuthenticationSignIn request) : IRequest<ResponseAuthenticationSignIn>
{
    public string Email { get; } = request.Email;
    public string Password { get; } = request.Password;
}
