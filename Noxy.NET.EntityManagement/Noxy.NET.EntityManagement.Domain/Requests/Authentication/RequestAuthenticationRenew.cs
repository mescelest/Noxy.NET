using Noxy.NET.EntityManagement.Domain.Abstractions.Requests;
using Noxy.NET.EntityManagement.Domain.Responses.Authentication;

namespace Noxy.NET.EntityManagement.Domain.Requests.Authentication;

public class RequestAuthenticationRenew : BaseRequestPost<ResponseAuthenticationRenew>
{
    public override string APIEndpoint => "Authentication/Renew";
}
