using MediatR;
using Noxy.NET.EntityManagement.Domain.Requests;
using Noxy.NET.EntityManagement.Domain.Responses;

namespace Noxy.NET.EntityManagement.API.Queries;

public class QueryAuthenticationSignIn(RequestAuthenticationSignIn request) : IRequest<ResponseAuthenticationSignIn>
{
    public string Email { get; } = request.Email;
    public string Password { get; } = request.Password;
}
