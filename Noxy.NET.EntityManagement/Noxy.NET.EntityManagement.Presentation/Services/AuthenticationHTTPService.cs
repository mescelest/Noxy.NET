using System.Net.Http.Json;
using Noxy.NET.EntityManagement.Domain.Models.Forms.Authentication;
using Noxy.NET.EntityManagement.Domain.Models.Responses.Authentication;
using Noxy.NET.EntityManagement.Presentation.Abstractions;

namespace Noxy.NET.EntityManagement.Presentation.Services;

public class AuthenticationHttpClient(HttpClient http, UserAuthenticationStateProvider serviceAuthentication) : BaseHttpClient(http, serviceAuthentication)
{
    public async Task<SignUpResponse> SignUpUser(AuthenticationSignUpAPIFormModel model)
    {
        HttpContent response = ExtractResponse(await SendRequest(model));
        return await response.ReadFromJsonAsync<SignUpResponse>() ?? throw new FormatException();
    }

    public async Task<SignInResponse> SignInUser(AuthenticationSignInAPIFormModel model)
    {
        HttpContent response = ExtractResponse(await SendRequest(model));
        return await response.ReadFromJsonAsync<SignInResponse>() ?? throw new FormatException();
    }

    public async Task<string> RenewUser()
    {
        HttpRequestMessage requestMessage = CreateRequest(HttpMethod.Post, "User/Renew");
        HttpContent response = ExtractResponse(await SendRequest(requestMessage));
        return await response.ReadAsStringAsync();
    }
}
