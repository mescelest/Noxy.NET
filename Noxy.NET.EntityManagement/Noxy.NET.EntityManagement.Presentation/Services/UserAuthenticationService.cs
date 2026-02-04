using Noxy.NET.EntityManagement.Domain.Models.Responses.Authentication;

namespace Noxy.NET.EntityManagement.Presentation.Services;

public class UserAuthenticationService(APIHttpClient api, UserAuthenticationStateProvider authProvider)
{
    public async Task Renew()
    {
        if (authProvider.Identity == null) return;
        try
        {
            AuthenticationRenewResponse response = await api.SendRequest<AuthenticationRenewResponse>(HttpMethod.Post, "Authentication/Renew");
            await authProvider.UpdateIdentity(response.JWT);
        }
        catch
        {
            await authProvider.ResetIdentity();
        }
    }
}
