using Noxy.NET.EntityManagement.Domain.Models.Forms.Authentication;
using Noxy.NET.EntityManagement.Domain.Models.Responses.Authentication;
using Noxy.NET.EntityManagement.Presentation.Abstractions;

namespace Noxy.NET.EntityManagement.Presentation.Services.HttpClients;

public class AuthenticationHttpClient(HttpClient http, UserAuthenticationStateProvider serviceAuthentication) : BaseHttpClientForm(http, serviceAuthentication)
{
    public async Task<SignUpResponse> SignUpUser(AuthenticationSignUpAPIFormModel model)
    {
        return await SendRequest<SignUpResponse>(model);
    }

    public async Task<SignInResponse> SignInUser(AuthenticationSignInAPIFormModel model)
    {
        return await SendRequest<SignInResponse>(model);
    }

    public async Task<string> RenewUser()
    {
        HttpRequestMessage request = CreateRequest(HttpMethod.Post, "User/Renew");
        return await SendRequest<string>(request);
    }
}
