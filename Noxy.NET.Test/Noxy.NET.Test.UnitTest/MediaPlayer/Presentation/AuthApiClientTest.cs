using System.Net;
using System.Text.Json;
using Noxy.NET.MediaPlayer.Domain.Responses.Authentication;
using Noxy.NET.MediaPlayer.Presentation.Exceptions;
using Noxy.NET.MediaPlayer.Presentation.Services;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class AuthApiClientTest
{
    private const string Tokens = "{\"accessToken\":\"access\",\"refreshToken\":\"refresh\"}";

    private readonly FakeHttpMessageHandler _handler = new();
    private readonly FakeServerAddressStorage _address = new();
    private readonly AuthApiClient _client;

    public AuthApiClientTest()
    {
        _handler.Respond = _ => FakeHttpMessageHandler.Json(HttpStatusCode.OK, Tokens);
        _client = new(new(_handler), _address);
    }

    private FakeHttpMessageHandler.RecordedRequest Single()
    {
        return Assert.Single(_handler.RequestList);
    }

    private static JsonElement Body(FakeHttpMessageHandler.RecordedRequest request)
    {
        return JsonDocument.Parse(request.Body ?? throw new InvalidOperationException("The request has no body.")).RootElement;
    }

    [Fact]
    public async Task SignUp_PostsTheEmailAndPasswordToTheSavedAddress_WithNoAuthorization_AndReturnsTheTokens()
    {
        ResponseAuthentication tokens = await _client.SignUp("a@example.com", "a-long-enough-password");

        FakeHttpMessageHandler.RecordedRequest request = Single();
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal("https://server.example/Auth/SignUp", request.Uri?.ToString());
        Assert.Null(request.Authorization);
        Assert.Equal("a@example.com", Body(request).GetProperty("email").GetString());
        Assert.Equal("a-long-enough-password", Body(request).GetProperty("password").GetString());
        Assert.Equal("access", tokens.AccessToken);
        Assert.Equal("refresh", tokens.RefreshToken);
    }

    [Fact]
    public async Task SignIn_PostsToSignIn()
    {
        await _client.SignIn("a@example.com", "pw");

        Assert.Equal("https://server.example/Auth/SignIn", Single().Uri?.ToString());
        Assert.Equal("pw", Body(Single()).GetProperty("password").GetString());
    }

    [Fact]
    public async Task SignInGoogle_PostsTheIdTokenToGoogle()
    {
        await _client.SignInGoogle("the-id-token");

        Assert.Equal("https://server.example/Auth/Google", Single().Uri?.ToString());
        Assert.Equal("the-id-token", Body(Single()).GetProperty("idToken").GetString());
    }

    [Fact]
    public async Task Refresh_PostsTheRefreshTokenToRefresh()
    {
        await _client.Refresh("old-refresh");

        Assert.Equal("https://server.example/Auth/Refresh", Single().Uri?.ToString());
        Assert.Equal("old-refresh", Body(Single()).GetProperty("refreshToken").GetString());
    }

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized, "Invalid credentials.")]
    [InlineData(HttpStatusCode.Conflict, "The email is already in use.")]
    [InlineData(HttpStatusCode.ServiceUnavailable, "Google login is not configured.")]
    public async Task AProblemResponse_ThrowsApiExceptionWithItsStatusAndTitle(HttpStatusCode status, string title)
    {
        _handler.Respond = _ => FakeHttpMessageHandler.Problem(status, title);

        ApiException exception = await Assert.ThrowsAsync<ApiException>(() => _client.SignIn("a@example.com", "pw"));

        Assert.Equal(status, exception.Status);
        Assert.Equal(title, exception.Title);
    }

    [Fact]
    public async Task WithNoSavedAddress_ThrowsServerAddressMissing_AndSendsNothing()
    {
        _address.Address = null;

        await Assert.ThrowsAsync<ServerAddressMissingException>(() => _client.SignIn("a@example.com", "pw"));

        Assert.Empty(_handler.RequestList);
    }

    [Fact]
    public async Task ANewAddress_AppliesToTheNextCall()
    {
        await _client.Refresh("r");
        await _address.SetAddress("http://other.example:8080/");
        await _client.Refresh("r");

        Assert.Equal(["https://server.example/Auth/Refresh", "http://other.example:8080/Auth/Refresh"], _handler.RequestList.Select(x => x.Uri?.ToString()));
    }

    [Fact]
    public async Task ANetworkFailure_PropagatesAsHttpRequestException()
    {
        _handler.Failure = new HttpRequestException("The network is down.");

        await Assert.ThrowsAsync<HttpRequestException>(() => _client.Refresh("r"));
    }

    [Fact]
    public async Task ASuccessBodyThatIsNotTokens_ThrowsApiException_WithoutEchoingTheBody()
    {
        _handler.Respond = _ => FakeHttpMessageHandler.Json(HttpStatusCode.OK, "null");

        ApiException exception = await Assert.ThrowsAsync<ApiException>(() => _client.Refresh("secret-refresh-token"));

        Assert.DoesNotContain("secret-refresh-token", exception.Message);
    }
}
