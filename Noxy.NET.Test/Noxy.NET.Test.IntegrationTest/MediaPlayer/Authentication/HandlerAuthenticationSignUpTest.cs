using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Responses.Authentication;
using Noxy.NET.Test.MediaPlayer.Fixtures;

namespace Noxy.NET.Test.MediaPlayer.Authentication;

public class HandlerAuthenticationSignUpTest : IDisposable
{
    private const string Password = "a-long-enough-password";

    private readonly DatabaseFixture _fixture = new();

    public void Dispose()
    {
        _fixture.Dispose();
    }

    private Task<ResponseAuthentication> SignUp(string email = "user@example.com")
    {
        return _fixture.SignUp.Handle(new(email, Password), TestContext.Current.CancellationToken).AsTask();
    }

    private async Task<User?> FindUser(string email)
    {
        await using IUnitOfWork uow = _fixture.UoWFactory.Create();
        return await uow.Authentication.FindUserByEmail(email);
    }

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
        await SignUp();

        await Assert.ThrowsAsync<EmailAlreadyInUseException>(() => SignUp("USER@example.com"));
    }
}
