using Noxy.NET.EntityManagement.Domain.Abstractions.Requests;
using Noxy.NET.EntityManagement.Domain.Models.Responses.Authentication;

namespace Noxy.NET.EntityManagement.Domain.Requests;

public class RequestAuthenticationRenew : BaseRequestPost<AuthenticationRenewResponse>
{
    public override string APIEndpoint => "Authentication/Renew";
}
