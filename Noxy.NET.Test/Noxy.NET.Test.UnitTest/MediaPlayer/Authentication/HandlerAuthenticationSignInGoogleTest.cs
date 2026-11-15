using Noxy.NET.MediaPlayer.Application.Commands.Authentication;
using Noxy.NET.MediaPlayer.Application.Handlers.Authentication;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Responses.Authentication;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Authentication;

public class HandlerAuthenticationSignInGoogleTest
{
    private readonly FakeGoogleTokenService _google = new();
    private readonly FakeStore _store = new();
    private readonly FakeTimeProvider _time = new();
    private readonly HandlerAuthenticationSignUp _signUp;
    private readonly HandlerAuthenticationSignInGoogle _handler;

    public HandlerAuthenticationSignInGoogleTest()
    {
        FakeUnitOfWorkFactory factory = new(_store);
        FakeTokenService serviceToken = new(_time);

        _signUp = new(factory, new FakePasswordHashService(), serviceToken);
        _handler = new(factory, _google, serviceToken);
    }

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    private Task<ResponseAuthentication> SignInGoogle(string token)
    {
        return _handler.Handle(new(token), Token).AsTask();
    }

    private void RegisterGoogle(string token, string subject, string email, bool verified = true)
    {
        _google.Register(token, new(subject, email, verified));
    }

    [Fact]
    public async Task Google_NewVerifiedAccount_CreatesVerifiedUserWithGoogleLogin()
    {
        RegisterGoogle("token", "sub-1", "New@Example.com");

        await SignInGoogle("token");

        EntityUser user = Assert.Single(_store.UserList);
        Assert.Equal("new@example.com", user.Email);
        Assert.True(user.IsEmailVerified);
        Assert.Null(user.PasswordHash);

        EntityExternalLogin login = Assert.Single(_store.ExternalLoginList);
        Assert.Equal(ExternalLoginProvider.Google, login.Provider);
        Assert.Equal("sub-1", login.ProviderKey);
        Assert.Equal(user.ID, login.UserID);
    }

    [Fact]
    public async Task Google_KnownSubject_SignsInTheLinkedUser_EvenIfTheEmailChanged()
    {
        RegisterGoogle("token", "sub-1", "old@example.com");
        await SignInGoogle("token");

        RegisterGoogle("token2", "sub-1", "changed@example.com");
        ResponseAuthentication response = await SignInGoogle("token2");

        Assert.Single(_store.UserList);
        Assert.Equal("access-for-old@example.com", response.AccessToken);
    }

    [Fact]
    public async Task Google_UnverifiedEmail_ThrowsAndStoresNothing()
    {
        RegisterGoogle("token", "sub-1", "new@example.com", false);

        await Assert.ThrowsAsync<AuthenticationFailedException>(() => SignInGoogle("token"));

        Assert.Empty(_store.UserList);
        Assert.Empty(_store.ExternalLoginList);
        Assert.Equal(0, _store.CommitCount);
    }

    [Fact]
    public async Task Google_ExistingVerifiedUser_GetsGoogleLinked()
    {
        _store.UserList.Add(new() { Email = "user@example.com", IsEmailVerified = true, PasswordHash = "hashed:x" });
        RegisterGoogle("token", "sub-1", "user@example.com");

        await SignInGoogle("token");

        Assert.Single(_store.UserList);
        EntityExternalLogin login = Assert.Single(_store.ExternalLoginList);
        Assert.Equal(_store.UserList[0].ID, login.UserID);
    }

    [Fact]
    public async Task Google_ExistingUnverifiedUser_Throws_SoWhoeverRegisteredFirstCannotKeepTheAccount()
    {
        await _signUp.Handle(new("user@example.com", "a-long-enough-password"), Token);
        RegisterGoogle("token", "sub-1", "user@example.com");

        await Assert.ThrowsAsync<EmailAlreadyInUseException>(() => SignInGoogle("token"));

        Assert.Empty(_store.ExternalLoginList);
    }

    [Fact]
    public async Task Google_InvalidToken_Throws()
    {
        await Assert.ThrowsAsync<AuthenticationFailedException>(() => SignInGoogle("unknown"));
    }

    [Fact]
    public void ToString_ShowsNoIdToken()
    {
        string text = new CommandAuthenticationSignInGoogle("secret-id-token").ToString();

        Assert.DoesNotContain("secret-id-token", text);
    }
}
