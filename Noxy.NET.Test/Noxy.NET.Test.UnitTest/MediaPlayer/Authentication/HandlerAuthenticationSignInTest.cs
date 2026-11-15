using Noxy.NET.MediaPlayer.Application.Commands.Authentication;
using Noxy.NET.MediaPlayer.Application.Handlers.Authentication;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Responses.Authentication;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Authentication;

public class HandlerAuthenticationSignInTest
{
    private const string Password = "a-long-enough-password";

    private readonly FakePasswordHashService _passwords = new();
    private readonly FakeStore _store = new();
    private readonly FakeTimeProvider _time = new();
    private readonly HandlerAuthenticationSignUp _signUp;
    private readonly HandlerAuthenticationSignIn _handler;

    public HandlerAuthenticationSignInTest()
    {
        FakeUnitOfWorkFactory factory = new(_store);
        FakeTokenService serviceToken = new(_time);

        _signUp = new(factory, _passwords, serviceToken);
        _handler = new(factory, _passwords, serviceToken);
    }

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    private Task<ResponseAuthentication> SignUp(string email = "user@example.com")
    {
        return _signUp.Handle(new(email, Password), Token).AsTask();
    }

    [Fact]
    public async Task SignIn_UnknownEmail_StillVerifiesAPassword_SoTimingDoesNotLeakWhichEmailsExist()
    {
        await Assert.ThrowsAsync<AuthenticationFailedException>(() => _handler.Handle(new("nobody@example.com", "whatever"), Token).AsTask());

        Assert.Equal([null], _passwords.VerifiedHashes);
    }

    [Fact]
    public async Task SignIn_WrongPassword_ThrowsAndIssuesNoToken()
    {
        await SignUp();
        int tokens = _store.RefreshTokenList.Count;

        await Assert.ThrowsAsync<AuthenticationFailedException>(() => _handler.Handle(new("user@example.com", "wrong"), Token).AsTask());

        Assert.Equal(tokens, _store.RefreshTokenList.Count);
    }

    [Fact]
    public async Task SignIn_NormalizesEmail()
    {
        await SignUp();

        ResponseAuthentication response = await _handler.Handle(new(" USER@example.com ", Password), Token);

        Assert.False(string.IsNullOrEmpty(response.AccessToken));
    }

    [Fact]
    public void ToString_ShowsNoPassword()
    {
        string text = new CommandAuthenticationSignIn("user@example.com", Password).ToString();

        Assert.DoesNotContain(Password, text);
    }
}
