using System.Net;
using System.Net.Http.Json;
using Noxy.NET.MediaPlayer.Application.Requests.Authentication;
using Noxy.NET.MediaPlayer.Domain.Responses.Authentication;
using Noxy.NET.MediaPlayer.Presentation.Exceptions;
using Noxy.NET.MediaPlayer.Presentation.Extensions;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;

namespace Noxy.NET.MediaPlayer.Presentation.Services;

public class AuthApiClient(HttpClient client, IServerAddressStorage storageServerAddress) : IAuthApiClient
{
    private const string SignUpPath = "/Auth/SignUp";
    private const string SignInPath = "/Auth/SignIn";
    private const string GooglePath = "/Auth/Google";
    private const string RefreshPath = "/Auth/Refresh";

    public async Task<ResponseAuthentication> SignUp(string email, string password)
    {
        return await Post(SignUpPath, new RequestSignUp { Email = email, Password = password });
    }

    public async Task<ResponseAuthentication> SignIn(string email, string password)
    {
        return await Post(SignInPath, new RequestSignIn { Email = email, Password = password });
    }

    public async Task<ResponseAuthentication> SignInGoogle(string idToken)
    {
        return await Post(GooglePath, new RequestSignInGoogle { IdToken = idToken });
    }

    public async Task<ResponseAuthentication> Refresh(string refreshToken)
    {
        return await Post(RefreshPath, new RequestRefresh { RefreshToken = refreshToken });
    }

    // The address is read for every call, so a changed address applies to the next one. No bearer token is sent.
    private async Task<ResponseAuthentication> Post<TRequest>(string path, TRequest request)
    {
        string address = await storageServerAddress.GetAddress() ?? throw new ServerAddressMissingException();

        using HttpResponseMessage response = await client.PostAsJsonAsync($"{address}{path}", request);
        if (!response.IsSuccessStatusCode) throw await response.ToApiException();

        return await response.Content.ReadFromJsonAsync<ResponseAuthentication>() ?? throw new ApiException(HttpStatusCode.BadGateway, "The server sent an answer the app cannot read.");
    }
}
