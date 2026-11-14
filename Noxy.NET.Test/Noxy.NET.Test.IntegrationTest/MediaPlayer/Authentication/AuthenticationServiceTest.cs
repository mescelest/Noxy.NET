using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Responses.Authentication;
using Noxy.NET.Test.MediaPlayer.Fixtures;

namespace Noxy.NET.Test.MediaPlayer.Authentication;

public class AuthenticationServiceTest : IDisposable
{
    private const string Password = "a-long-enough-password";

    private readonly DatabaseFixture _fixture = new();

    public void Dispose()
    {
        _fixture.Dispose();
    }

    private Task<ResponseAuthentication> SignUp(string email = "user@example.com")
    {
        return _fixture.AuthenticationService.SignUp(new() { Email = email, Password = Password });
    }

    private async Task<User?> FindUser(string email)
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
        return _fixture.AuthenticationService.SignInWithGoogle(new() { IdToken = token });
    }

    #region -- Sign up --

    [Fact]
    public async Task SignUp_ReturnsTokens()
    {
        ResponseAuthentication response = await SignUp();

        Assert.False(string.IsNullOrEmpty(response.AccessToken));
        Assert.False(string.IsNullOrEmpty(response.RefreshToken));
    }

    [Fact]
    public async Task SignUp_NormalizesEmailAndStoresHashedPassword()
    {
        await SignUp("  User@Example.COM ");

        User? user = await FindUser("user@example.com");
        Assert.NotNull(user);
        Assert.False(user.IsEmailVerified);
        Assert.NotNull(user.PasswordHash);
        Assert.DoesNotContain(Password, user.PasswordHash);
    }

    [Fact]
    public async Task SignUp_DuplicateEmail_Throws()
    {
        await SignUp("user@example.com");

        await Assert.ThrowsAsync<EmailAlreadyInUseException>(() => SignUp("USER@example.com"));
    }

    #endregion -- Sign up --

    #region -- Sign in --

    [Fact]
    public async Task SignIn_CorrectPassword_ReturnsTokens()
    {
        await SignUp();

        ResponseAuthentication response = await _fixture.AuthenticationService.SignIn(new() { Email = "User@Example.com", Password = Password }, TestContext.Current.CancellationToken);

        Assert.False(string.IsNullOrEmpty(response.RefreshToken));
    }

    [Fact]
    public async Task SignIn_WrongPassword_Throws()
    {
        await SignUp();

        await Assert.ThrowsAsync<AuthenticationFailedException>(() => _fixture.AuthenticationService.SignIn(new() { Email = "user@example.com", Password = "not-the-password-at-all" }, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task SignIn_UnknownEmail_Throws()
    {
        await Assert.ThrowsAsync<AuthenticationFailedException>(() => _fixture.AuthenticationService.SignIn(new() { Email = "nobody@example.com", Password = Password }, TestContext.Current.CancellationToken));
    }

    #endregion -- Sign in --

    #region -- Google --

    [Fact]
    public async Task Google_NewVerifiedAccount_CreatesVerifiedUser()
    {
        RegisterGoogle("token", "sub-1", "New@Example.com");

        ResponseAuthentication response = await SignInGoogle("token");

        Assert.False(string.IsNullOrEmpty(response.AccessToken));
        User? user = await FindUser("new@example.com");
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
        await SignUp("user@example.com");
        RegisterGoogle("token", "sub-1", "user@example.com");

        await Assert.ThrowsAsync<EmailAlreadyInUseException>(() => SignInGoogle("token"));
    }

    #endregion -- Google --

    #region -- Refresh --

    [Fact]
    public async Task Refresh_ReturnsNewTokens()
    {
        ResponseAuthentication first = await SignUp();

        ResponseAuthentication second = await _fixture.AuthenticationService.Refresh(new() { RefreshToken = first.RefreshToken }, TestContext.Current.CancellationToken);

        Assert.NotEqual(first.RefreshToken, second.RefreshToken);
    }

    [Fact]
    public async Task Refresh_UnknownToken_Throws()
    {
        await Assert.ThrowsAsync<AuthenticationFailedException>(() => _fixture.AuthenticationService.Refresh(new() { RefreshToken = "nope" }, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Refresh_ReusedToken_RevokesEveryTokenOfTheUser()
    {
        ResponseAuthentication first = await SignUp();
        ResponseAuthentication second = await _fixture.AuthenticationService.Refresh(new() { RefreshToken = first.RefreshToken }, TestContext.Current.CancellationToken);

        // Presenting the already-used token means someone has a copy.
        await Assert.ThrowsAsync<AuthenticationFailedException>(() => _fixture.AuthenticationService.Refresh(new() { RefreshToken = first.RefreshToken }, TestContext.Current.CancellationToken));

        // The newer token must be dead too.
        await Assert.ThrowsAsync<AuthenticationFailedException>(() => _fixture.AuthenticationService.Refresh(new() { RefreshToken = second.RefreshToken }, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Refresh_ExpiredToken_Throws()
    {
        ResponseAuthentication first = await SignUp();

        _fixture.Time.Now = _fixture.Time.Now.AddDays(31);

        await Assert.ThrowsAsync<AuthenticationFailedException>(() => _fixture.AuthenticationService.Refresh(new() { RefreshToken = first.RefreshToken }, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task DeleteExpiredRefreshTokens_RemovesOnlyExpired()
    {
        ResponseAuthentication first = await SignUp();
        _fixture.Time.Now = _fixture.Time.Now.AddDays(31);
        ResponseAuthentication second = await _fixture.AuthenticationService.SignIn(new() { Email = "user@example.com", Password = Password }, TestContext.Current.CancellationToken);

        await using (IUnitOfWork uow = _fixture.UoWFactory.Create())
        {
            int deleted = await uow.Authentication.DeleteExpiredRefreshTokens(_fixture.Time.Now.UtcDateTime, TestContext.Current.CancellationToken);
            Assert.Equal(1, deleted);
        }

        // The surviving token still works, and the deleted one is gone.
        await _fixture.AuthenticationService.Refresh(new() { RefreshToken = second.RefreshToken }, TestContext.Current.CancellationToken);
        await Assert.ThrowsAsync<AuthenticationFailedException>(() => _fixture.AuthenticationService.Refresh(new() { RefreshToken = first.RefreshToken }, TestContext.Current.CancellationToken));
    }

    #endregion -- Refresh --
}
