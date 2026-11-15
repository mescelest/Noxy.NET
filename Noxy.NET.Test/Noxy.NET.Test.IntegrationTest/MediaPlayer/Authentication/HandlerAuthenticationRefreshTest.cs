using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Responses.Authentication;
using Noxy.NET.Test.MediaPlayer.Fixtures;

namespace Noxy.NET.Test.MediaPlayer.Authentication;

public class HandlerAuthenticationRefreshTest : IDisposable
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

    private Task<ResponseAuthentication> Refresh(string token)
    {
        return _fixture.Refresh.Handle(new(token), Token).AsTask();
    }

    [Fact]
    public async Task Refresh_ReturnsNewTokens()
    {
        ResponseAuthentication first = await SignUp();

        ResponseAuthentication second = await Refresh(first.RefreshToken);

        Assert.NotEqual(first.RefreshToken, second.RefreshToken);
    }

    [Fact]
    public async Task Refresh_UnknownToken_Throws()
    {
        await Assert.ThrowsAsync<AuthenticationFailedException>(() => Refresh("nope"));
    }

    [Fact]
    public async Task Refresh_ReusedToken_RevokesEveryTokenOfTheUser()
    {
        ResponseAuthentication first = await SignUp();
        ResponseAuthentication second = await Refresh(first.RefreshToken);

        // Presenting the already-used token means someone has a copy.
        await Assert.ThrowsAsync<AuthenticationFailedException>(() => Refresh(first.RefreshToken));

        // The newer token must be dead too.
        await Assert.ThrowsAsync<AuthenticationFailedException>(() => Refresh(second.RefreshToken));
    }

    [Fact]
    public async Task Refresh_ExpiredToken_Throws()
    {
        ResponseAuthentication first = await SignUp();

        _fixture.Time.Now = _fixture.Time.Now.AddDays(31);

        await Assert.ThrowsAsync<AuthenticationFailedException>(() => Refresh(first.RefreshToken));
    }

    [Fact]
    public async Task DeleteExpiredRefreshTokens_RemovesOnlyExpired()
    {
        ResponseAuthentication first = await SignUp();
        _fixture.Time.Now = _fixture.Time.Now.AddDays(31);
        ResponseAuthentication second = await _fixture.SignIn.Handle(new("user@example.com", Password), Token);

        await using (IUnitOfWork uow = _fixture.UoWFactory.Create())
        {
            int deleted = await uow.Authentication.DeleteExpiredRefreshTokens(_fixture.Time.Now.UtcDateTime, Token);
            Assert.Equal(1, deleted);
        }

        // The surviving token still works, and the deleted one is gone.
        await Refresh(second.RefreshToken);
        await Assert.ThrowsAsync<AuthenticationFailedException>(() => Refresh(first.RefreshToken));
    }
}
