using MediatR;
using Noxy.NET.EntityManagement.Domain.Responses;

namespace Noxy.NET.EntityManagement.API.Queries;

public class QueryAuthenticationRenew(string email) : IRequest<ResponseAuthenticationRenew>
{
    public string Email { get; } = email;
}
