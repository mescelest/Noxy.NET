using MediatR;
using Noxy.NET.EntityManagement.Domain.Requests;
using Noxy.NET.EntityManagement.Domain.Responses;

namespace Noxy.NET.EntityManagement.API.Queries;

public class QueryAuthenticationSignUp(RequestAuthenticationSignUp request) : IRequest<ResponseAuthenticationSignUp>
{
    public string Email { get; set; } = request.Email;
    public string Password { get; set; } = request.Password;
}
