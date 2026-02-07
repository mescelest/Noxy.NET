using Noxy.NET.EntityManagement.Domain.Abstractions.Requests;
using Noxy.NET.EntityManagement.Domain.Responses;

namespace Noxy.NET.EntityManagement.Domain.Requests;

public class RequestAuthenticationRenew : BaseRequestPost<ResponseAuthenticationRenew>
{
    public override string APIEndpoint => "Authentication/Renew";
}
