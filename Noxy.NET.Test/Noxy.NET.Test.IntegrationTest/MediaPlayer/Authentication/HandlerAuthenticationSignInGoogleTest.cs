using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Responses.Authentication;
using Noxy.NET.Test.MediaPlayer.Fixtures;

namespace Noxy.NET.Test.MediaPlayer.Authentication;

public class HandlerAuthenticationSignInGoogleTest : IDisposable
{
    private readonly DatabaseFixture _fixture = new();

    public void Dispose()
    {
        _fixture.Dispose();
    }

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    private async Task<EntityUser?> FindUser(string email)
    {
        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        return await uow.Authentication.FindUserByEmail(email);
    }

    private void RegisterGoogle(string token, string subject, string email, bool verified = true)
    {
        _fixture.GoogleTokenService.Register(token, new(subject, email, verified));
    }

    private Task<ResponseAuthentication> SignInGoogle(string token)
    {
        return _fixture.SignInGoogle.Handle(new(token), Token).AsTask();
    }

    [Fact]
    public async Task Google_NewVerifiedAccount_CreatesVerifiedUser()
    {
        RegisterGoogle("token", "sub-1", "New@Example.com");

        ResponseAuthentication response = await SignInGoogle("token");

        Assert.False(string.IsNullOrEmpty(response.AccessToken));
        EntityUser? user = await FindUser("new@example.com");
        Assert.NotNull(user);
        Assert.True(user.IsEmailVerified);
        Assert.Null(user.PasswordHash);
    }

    [Fact]
    public async Task Google_SecondSignIn_ReusesTheSameUser()
    {
        RegisterGoogle("token", "sub-1", "new@example.com");
        await SignInGoogle("token");

        // Same Google account with a different email: the subject is the identity, not the email.
        RegisterGoogle("token2", "sub-1", "changed@example.com");
        await SignInGoogle("token2");

        Assert.NotNull(await FindUser("new@example.com"));
        Assert.Null(await FindUser("changed@example.com"));
    }

    [Fact]
    public async Task Google_UnverifiedEmail_Throws()
    {
        RegisterGoogle("token", "sub-1", "new@example.com", false);

        await Assert.ThrowsAsync<AuthenticationFailedException>(() => SignInGoogle("token"));
        Assert.Null(await FindUser("new@example.com"));
    }

    [Fact]
    public async Task Google_InvalidToken_Throws()
    {
        await Assert.ThrowsAsync<AuthenticationFailedException>(() => SignInGoogle("unknown"));
    }

    [Fact]
    public async Task Google_ExistingUnverifiedPasswordAccount_ThrowsInsteadOfLinking()
    {
        await _fixture.SignUp.Handle(new("user@example.com", "a-long-enough-password"), Token);
        RegisterGoogle("token", "sub-1", "user@example.com");

        await Assert.ThrowsAsync<EmailAlreadyInUseException>(() => SignInGoogle("token"));
    }
}
