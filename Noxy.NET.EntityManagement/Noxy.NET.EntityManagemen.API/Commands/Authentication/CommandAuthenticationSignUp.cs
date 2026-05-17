using Mediator;
using Noxy.NET.EntityManagement.Domain.Requests.Authentication;
using Noxy.NET.EntityManagement.Domain.Responses.Authentication;

namespace Noxy.NET.EntityManagement.API.Commands.Authentication;

public class CommandAuthenticationSignUp(RequestAuthenticationSignUp request) : ICommand<ResponseAuthenticationSignUp>
{
    public string Email { get; set; } = request.Email;
    public string Password { get; set; } = request.Password;
}
