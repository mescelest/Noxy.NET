using Noxy.NET.EntityManagement.Domain.Models.Responses.Authentication;
using Noxy.NET.EntityManagement.Domain.Requests;

namespace Noxy.NET.EntityManagement.Presentation.Services;

public class UserAuthenticationService(APIHttpClient serviceHttp, UserAuthenticationStateProvider serviceAuth)
{
    public async Task Renew()
    {
        if (serviceAuth.Identity == null) return;
        try
        {
            AuthenticationRenewResponse response = await serviceHttp.SendRequest(new RequestAuthenticationRenew());
            await serviceAuth.UpdateIdentity(response.JWT);
        }
        catch
        {
            await serviceAuth.ResetIdentity();
        }
    }
}
