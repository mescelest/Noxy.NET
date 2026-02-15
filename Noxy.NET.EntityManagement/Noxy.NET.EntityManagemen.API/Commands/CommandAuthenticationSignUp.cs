using MediatR;
using Noxy.NET.EntityManagement.Domain.Requests;
using Noxy.NET.EntityManagement.Domain.Responses;

namespace Noxy.NET.EntityManagement.API.Commands;

public class CommandAuthenticationSignUp(RequestAuthenticationSignUp request) : IRequest<ResponseAuthenticationSignUp>
{
    public string Email { get; set; } = request.Email;
    public string Password { get; set; } = request.Password;
}
