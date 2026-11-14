using Noxy.NET.MediaPlayer.Application.Services;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Responses.Authentication;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Authentication;

public class AuthenticationServiceTest
{
    private readonly FakeGoogleTokenService _google = new();
    private readonly FakePasswordHashService _passwords = new();
    private readonly AuthenticationService _service;
    private readonly FakeStore _store = new();
    private readonly FakeTimeProvider _time = new();

    public AuthenticationServiceTest()
    {
        _service = new(new FakeUnitOfWorkFactory(_store), _passwords, new FakeTokenService(_time), _google, _time);
    }

    private Task<ResponseAuthentication> SignUp(string email = "user@example.com")
    {
        return _service.SignUp(new() { Email = email, Password = "a-long-enough-password" }, TestContext.Current.CancellationToken);
    }

    private Task<ResponseAuthentication> Refresh(string token)
    {
        return _service.Refresh(new() { RefreshToken = token }, TestContext.Current.CancellationToken);
    }

    private Task<ResponseAuthentication> SignInGoogle(string token)
    {
        return _service.SignInWithGoogle(new() { IdToken = token }, TestContext.Current.CancellationToken);
    }

    private void RegisterGoogle(string token, string subject, string email, bool verified = true)
    {
        _google.Register(token, new(subject, email, verified));
    }

    #region -- Sign up --

    [Fact]
    public async Task SignUp_StoresNormalizedUserWithHashedPassword_AndOneRefreshToken()
    {
        ResponseAuthentication response = await SignUp("  User@Example.COM ");

        User user = Assert.Single(_store.Users);
        Assert.Equal("user@example.com", user.Email);
        Assert.Equal("hashed:a-long-enough-password", user.PasswordHash);
        Assert.False(user.IsEmailVerified);

        RefreshToken token = Assert.Single(_store.RefreshTokens);
        Assert.Equal(user.ID, token.UserID);
        Assert.Equal("hash-of-" + response.RefreshToken, token.TokenHash);
        Assert.Equal(1, _store.CommitCount);
    }

    [Fact]
    public async Task SignUp_DuplicateEmail_ThrowsAndCommitsNothing()
    {
        await SignUp("user@example.com");
        int commits = _store.CommitCount;

        await Assert.ThrowsAsync<EmailAlreadyInUseException>(() => SignUp("USER@example.com"));

        Assert.Equal(commits, _store.CommitCount);
        Assert.Single(_store.Users);
    }

    #endregion -- Sign up --

    #region -- Sign in --

    [Fact]
    public async Task SignIn_UnknownEmail_StillVerifiesAPassword_SoTimingDoesNotLeakWhichEmailsExist()
    {
        await Assert.ThrowsAsync<AuthenticationFailedException>(() => _service.SignIn(new() { Email = "nobody@example.com", Password = "whatever" }, TestContext.Current.CancellationToken));

        Assert.Equal([null], _passwords.VerifiedHashes);
    }

    [Fact]
    public async Task SignIn_WrongPassword_ThrowsAndIssuesNoToken()
    {
        await SignUp();
        int tokens = _store.RefreshTokens.Count;

        await Assert.ThrowsAsync<AuthenticationFailedException>(() => _service.SignIn(new() { Email = "user@example.com", Password = "wrong" }, TestContext.Current.CancellationToken));

        Assert.Equal(tokens, _store.RefreshTokens.Count);
    }

    [Fact]
    public async Task SignIn_NormalizesEmail()
    {
        await SignUp("user@example.com");

        ResponseAuthentication response = await _service.SignIn(new() { Email = " USER@example.com ", Password = "a-long-enough-password" }, TestContext.Current.CancellationToken);

        Assert.False(string.IsNullOrEmpty(response.AccessToken));
    }

    #endregion -- Sign in --

    #region -- Google --

    [Fact]
    public async Task Google_NewVerifiedAccount_CreatesVerifiedUserWithGoogleLogin()
    {
        RegisterGoogle("token", "sub-1", "New@Example.com");

        await SignInGoogle("token");

        User user = Assert.Single(_store.Users);
        Assert.Equal("new@example.com", user.Email);
        Assert.True(user.IsEmailVerified);
        Assert.Null(user.PasswordHash);

        ExternalLogin login = Assert.Single(_store.ExternalLogins);
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

        Assert.Single(_store.Users);
        Assert.Equal("access-for-old@example.com", response.AccessToken);
    }

    [Fact]
    public async Task Google_UnverifiedEmail_ThrowsAndStoresNothing()
    {
        RegisterGoogle("token", "sub-1", "new@example.com", false);

        await Assert.ThrowsAsync<AuthenticationFailedException>(() => SignInGoogle("token"));

        Assert.Empty(_store.Users);
        Assert.Empty(_store.ExternalLogins);
        Assert.Equal(0, _store.CommitCount);
    }

    [Fact]
    public async Task Google_ExistingVerifiedUser_GetsGoogleLinked()
    {
        _store.Users.Add(new() { Email = "user@example.com", IsEmailVerified = true, PasswordHash = "hashed:x" });
        RegisterGoogle("token", "sub-1", "user@example.com");

        await SignInGoogle("token");

        Assert.Single(_store.Users);
        ExternalLogin login = Assert.Single(_store.ExternalLogins);
        Assert.Equal(_store.Users[0].ID, login.UserID);
    }

    [Fact]
    public async Task Google_ExistingUnverifiedUser_Throws_SoWhoeverRegisteredFirstCannotKeepTheAccount()
    {
        await SignUp("user@example.com");
        RegisterGoogle("token", "sub-1", "user@example.com");

        await Assert.ThrowsAsync<EmailAlreadyInUseException>(() => SignInGoogle("token"));

        Assert.Empty(_store.ExternalLogins);
    }

    [Fact]
    public async Task Google_InvalidToken_Throws()
    {
        await Assert.ThrowsAsync<AuthenticationFailedException>(() => SignInGoogle("unknown"));
    }

    #endregion -- Google --

    #region -- Refresh --

    [Fact]
    public async Task Refresh_RevokesTheUsedTokenAndIssuesANewOne()
    {
        ResponseAuthentication first = await SignUp();

        ResponseAuthentication second = await Refresh(first.RefreshToken);

        Assert.NotEqual(first.RefreshToken, second.RefreshToken);
        Assert.Equal(2, _store.RefreshTokens.Count);
        Assert.True(_store.RefreshTokens.Single(x => x.TokenHash == "hash-of-" + first.RefreshToken).IsRevoked);
        Assert.False(_store.RefreshTokens.Single(x => x.TokenHash == "hash-of-" + second.RefreshToken).IsRevoked);
    }

    [Fact]
    public async Task Refresh_UnknownToken_Throws()
    {
        await Assert.ThrowsAsync<AuthenticationFailedException>(() => Refresh("nope"));
    }

    [Fact]
    public async Task Refresh_ReusedToken_RevokesEverythingTheUserHas()
    {
        ResponseAuthentication first = await SignUp();
        ResponseAuthentication second = await Refresh(first.RefreshToken);

        await Assert.ThrowsAsync<AuthenticationFailedException>(() => Refresh(first.RefreshToken));

        Assert.Equal(1, _store.RevokeAllCount);
        Assert.All(_store.RefreshTokens, x => Assert.True(x.IsRevoked));
        await Assert.ThrowsAsync<AuthenticationFailedException>(() => Refresh(second.RefreshToken));
    }

    [Fact]
    public async Task Refresh_ExpiredToken_ThrowsWithoutRevokingAnything()
    {
        ResponseAuthentication first = await SignUp();
        _time.Now = _time.Now.AddDays(31);

        await Assert.ThrowsAsync<AuthenticationFailedException>(() => Refresh(first.RefreshToken));

        Assert.Equal(0, _store.RevokeAllCount);
        Assert.False(_store.RefreshTokens.Single().IsRevoked);
    }

    [Fact]
    public async Task Refresh_LosingTheRaceToAnotherRequest_CountsAsReuse()
    {
        ResponseAuthentication first = await SignUp();
        _store.LoseRevokeRace = true;

        await Assert.ThrowsAsync<AuthenticationFailedException>(() => Refresh(first.RefreshToken));

        Assert.Equal(1, _store.RevokeAllCount);
    }

    #endregion -- Refresh --
}
