using Noxy.NET.MediaPlayer.Application.Commands.Authentication;
using Noxy.NET.MediaPlayer.Application.Handlers.Authentication;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Responses.Authentication;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Authentication;

public class HandlerAuthenticationRefreshTest
{
    private readonly FakeStore _store = new();
    private readonly FakeTimeProvider _time = new();
    private readonly HandlerAuthenticationSignUp _signUp;
    private readonly HandlerAuthenticationRefresh _handler;

    public HandlerAuthenticationRefreshTest()
    {
        FakeUnitOfWorkFactory factory = new(_store);
        FakeTokenService serviceToken = new(_time);

        _signUp = new(factory, new FakePasswordHashService(), serviceToken);
        _handler = new(factory, serviceToken, _time);
    }

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    private Task<ResponseAuthentication> SignUp()
    {
        return _signUp.Handle(new("user@example.com", "a-long-enough-password"), Token).AsTask();
    }

    private Task<ResponseAuthentication> Refresh(string token)
    {
        return _handler.Handle(new(token), Token).AsTask();
    }

    [Fact]
    public async Task Refresh_RevokesTheUsedTokenAndIssuesANewOne()
    {
        ResponseAuthentication first = await SignUp();

        ResponseAuthentication second = await Refresh(first.RefreshToken);

        Assert.NotEqual(first.RefreshToken, second.RefreshToken);
        Assert.Equal(2, _store.RefreshTokenList.Count);
        Assert.True(_store.RefreshTokenList.Single(x => x.TokenHash == "hash-of-" + first.RefreshToken).IsRevoked);
        Assert.False(_store.RefreshTokenList.Single(x => x.TokenHash == "hash-of-" + second.RefreshToken).IsRevoked);
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
        Assert.All(_store.RefreshTokenList, x => Assert.True(x.IsRevoked));
        await Assert.ThrowsAsync<AuthenticationFailedException>(() => Refresh(second.RefreshToken));
    }

    [Fact]
    public async Task Refresh_ExpiredToken_ThrowsWithoutRevokingAnything()
    {
        ResponseAuthentication first = await SignUp();
        _time.Now = _time.Now.AddDays(31);

        await Assert.ThrowsAsync<AuthenticationFailedException>(() => Refresh(first.RefreshToken));

        Assert.Equal(0, _store.RevokeAllCount);
        Assert.False(_store.RefreshTokenList.Single().IsRevoked);
    }

    [Fact]
    public async Task Refresh_LosingTheRaceToAnotherRequest_CountsAsReuse()
    {
        ResponseAuthentication first = await SignUp();
        _store.LoseRevokeRace = true;

        await Assert.ThrowsAsync<AuthenticationFailedException>(() => Refresh(first.RefreshToken));

        Assert.Equal(1, _store.RevokeAllCount);
    }

    [Fact]
    public async Task Refresh_ATokenOfAUserThatNoLongerExists_ThrowsAuthenticationFailed()
    {
        ResponseAuthentication first = await SignUp();
        _store.UserList.Clear();

        await Assert.ThrowsAsync<AuthenticationFailedException>(() => Refresh(first.RefreshToken));
    }

    [Fact]
    public void ToString_ShowsNoRefreshToken()
    {
        string text = new CommandAuthenticationRefresh("secret-refresh-token").ToString();

        Assert.DoesNotContain("secret-refresh-token", text);
    }
}
