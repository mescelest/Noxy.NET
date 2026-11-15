using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Responses.Authentication;
using Noxy.NET.Test.MediaPlayer.Fixtures;

namespace Noxy.NET.Test.MediaPlayer.Authentication;

public class HandlerAuthenticationSignInTest : IDisposable
{
    private const string Password = "a-long-enough-password";

    private readonly DatabaseFixture _fixture = new();

    public void Dispose()
    {
        _fixture.Dispose();
    }

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    private Task<ResponseAuthentication> SignUp()
    {
        return _fixture.SignUp.Handle(new("user@example.com", Password), Token).AsTask();
    }

    [Fact]
    public async Task SignIn_CorrectPassword_ReturnsTokens()
    {
        await SignUp();

        ResponseAuthentication response = await _fixture.SignIn.Handle(new("User@Example.com", Password), Token);

        Assert.False(string.IsNullOrEmpty(response.RefreshToken));
    }

    [Fact]
    public async Task SignIn_WrongPassword_Throws()
    {
        await SignUp();

        await Assert.ThrowsAsync<AuthenticationFailedException>(() => _fixture.SignIn.Handle(new("user@example.com", "not-the-password-at-all"), Token).AsTask());
    }

    [Fact]
    public async Task SignIn_UnknownEmail_Throws()
    {
        await Assert.ThrowsAsync<AuthenticationFailedException>(() => _fixture.SignIn.Handle(new("nobody@example.com", Password), Token).AsTask());
    }
}
