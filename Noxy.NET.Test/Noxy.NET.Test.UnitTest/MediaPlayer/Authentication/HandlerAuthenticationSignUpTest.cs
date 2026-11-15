using Noxy.NET.MediaPlayer.Application.Commands.Authentication;
using Noxy.NET.MediaPlayer.Application.Handlers.Authentication;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Responses.Authentication;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Authentication;

public class HandlerAuthenticationSignUpTest
{
    private const string Password = "a-long-enough-password";

    private readonly FakeStore _store = new();
    private readonly FakeTimeProvider _time = new();
    private readonly HandlerAuthenticationSignUp _handler;

    public HandlerAuthenticationSignUpTest()
    {
        _handler = new(new FakeUnitOfWorkFactory(_store), new FakePasswordHashService(), new FakeTokenService(_time));
    }

    private Task<ResponseAuthentication> SignUp(string email = "user@example.com")
    {
        return _handler.Handle(new(email, Password), TestContext.Current.CancellationToken).AsTask();
    }

    [Fact]
    public async Task SignUp_StoresNormalizedUserWithHashedPassword_AndOneRefreshToken()
    {
        ResponseAuthentication response = await SignUp("  User@Example.COM ");

        EntityUser user = Assert.Single(_store.UserList);
        Assert.Equal("user@example.com", user.Email);
        Assert.Equal("hashed:a-long-enough-password", user.PasswordHash);
        Assert.False(user.IsEmailVerified);

        EntityRefreshToken token = Assert.Single(_store.RefreshTokenList);
        Assert.Equal(user.ID, token.UserID);
        Assert.Equal("hash-of-" + response.RefreshToken, token.TokenHash);
        Assert.Equal(1, _store.CommitCount);
    }

    [Fact]
    public async Task SignUp_DuplicateEmail_ThrowsAndCommitsNothing()
    {
        await SignUp();
        int commits = _store.CommitCount;

        await Assert.ThrowsAsync<EmailAlreadyInUseException>(() => SignUp("USER@example.com"));

        Assert.Equal(commits, _store.CommitCount);
        Assert.Single(_store.UserList);
    }

    [Fact]
    public void ToString_ShowsNoPassword()
    {
        string text = new CommandAuthenticationSignUp("user@example.com", Password).ToString();

        Assert.DoesNotContain(Password, text);
    }
}
