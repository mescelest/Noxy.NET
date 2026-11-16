using System.Net;
using Noxy.NET.MediaPlayer.Domain.Responses.Authentication;
using Noxy.NET.MediaPlayer.Presentation.Exceptions;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;
using Noxy.NET.Test.MediaPlayer.Fixtures;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeAuthApiClient(TimeProvider time) : IAuthApiClient
{
    private int _serial;

    // What was called, in order, for example "SignIn:a@example.com" or "Refresh:refresh-1".
    public List<string> CallList { get; } = [];

    public int RefreshCount { get; private set; }

    // The access token of the most recent answer, or null before any.
    public string? LastAccessToken { get; private set; }

    // Fails the next calls with this, for as long as it is set.
    public Exception? Failure { get; set; }

    // When set, a refresh waits for it before answering, so tests can hold several callers at once.
    public Task? RefreshGate { get; set; }

    // How long the tokens this fake hands out last.
    public TimeSpan Lifetime { get; set; } = TimeSpan.FromMinutes(15);

    public static ApiException Rejected(HttpStatusCode status = HttpStatusCode.Unauthorized)
    {
        return new(status, "Invalid credentials.");
    }

    public Task<ResponseAuthentication> SignUp(string email, string password)
    {
        return Respond($"SignUp:{email}");
    }

    public Task<ResponseAuthentication> SignIn(string email, string password)
    {
        return Respond($"SignIn:{email}");
    }

    public Task<ResponseAuthentication> SignInGoogle(string idToken)
    {
        return Respond($"SignInGoogle:{idToken}");
    }

    public async Task<ResponseAuthentication> Refresh(string refreshToken)
    {
        RefreshCount++;
        if (RefreshGate is not null) await RefreshGate;

        return await Respond($"Refresh:{refreshToken}");
    }

    private Task<ResponseAuthentication> Respond(string call)
    {
        CallList.Add(call);
        if (Failure is not null) return Task.FromException<ResponseAuthentication>(Failure);

        _serial++;
        LastAccessToken = AccessTokenFactory.Create(time.GetUtcNow().UtcDateTime.Add(Lifetime), $"access-{_serial}");
        return Task.FromResult(new ResponseAuthentication
        {
            AccessToken = LastAccessToken,
            RefreshToken = $"refresh-{_serial}"
        });
    }
}
