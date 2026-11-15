using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Noxy.NET.MediaPlayer.Application.Configurations;
using Noxy.NET.MediaPlayer.Application.Models.Authentication;
using Noxy.NET.MediaPlayer.Application.Services;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.Test.MediaPlayer.Fakes;
using RefreshToken = Noxy.NET.MediaPlayer.Application.Models.Authentication.RefreshToken;

namespace Noxy.NET.Test.MediaPlayer.Authentication;

public class TokenServiceTest
{
    private readonly ConfigJwt _config = new()
    {
        Secret = new('s', 64),
        Issuer = "test-issuer",
        Audience = "test-audience",
        AccessTokenMinutes = 15,
        RefreshTokenDays = 30
    };

    private readonly FakeTimeProvider _time = new();
    private readonly TokenService _service;

    public TokenServiceTest()
    {
        _service = new(Options.Create(_config), _time);
    }

    private TokenValidationParameters Parameters(string secret)
    {
        return new()
        {
            ValidIssuer = _config.Issuer,
            ValidAudience = _config.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            ClockSkew = TimeSpan.Zero,
            LifetimeValidator = (_, expires, _, _) => expires > _time.Now.UtcDateTime
        };
    }

    #region -- Access token --

    [Fact]
    public async Task AccessToken_IsSignedAndCarriesTheUsersIdentity()
    {
        User user = new() { Email = "user@example.com" };

        AccessToken token = _service.CreateAccessToken(user);
        TokenValidationResult result = await new JsonWebTokenHandler().ValidateTokenAsync(token.Token, Parameters(_config.Secret));

        Assert.True(result.IsValid);
        JsonWebToken jwt = (JsonWebToken)result.SecurityToken;
        Assert.Equal(user.ID.ToString(), jwt.Subject);
        Assert.Equal("user@example.com", jwt.GetClaim(JwtRegisteredClaimNames.Email).Value);
    }

    [Fact]
    public async Task AccessToken_ExpiresAfterTheConfiguredMinutes()
    {
        AccessToken token = _service.CreateAccessToken(new() { Email = "user@example.com" });

        Assert.Equal(_time.Now.UtcDateTime.AddMinutes(15), token.TimeExpires);

        _time.Now = _time.Now.AddMinutes(16);
        TokenValidationResult result = await new JsonWebTokenHandler().ValidateTokenAsync(token.Token, Parameters(_config.Secret));
        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task AccessToken_IsRejectedWithAnotherSecret()
    {
        AccessToken token = _service.CreateAccessToken(new() { Email = "user@example.com" });

        TokenValidationResult result = await new JsonWebTokenHandler().ValidateTokenAsync(token.Token, Parameters(new('z', 64)));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void AccessToken_IsUniquePerCall()
    {
        User user = new() { Email = "user@example.com" };

        Assert.NotEqual(_service.CreateAccessToken(user).Token, _service.CreateAccessToken(user).Token);
    }

    #endregion -- Access token --

    #region -- Refresh token --

    [Fact]
    public void RefreshToken_IsRandomAndUrlSafe()
    {
        RefreshToken first = _service.CreateRefreshToken();
        RefreshToken second = _service.CreateRefreshToken();

        Assert.NotEqual(first.Token, second.Token);
        Assert.Matches("^[A-Za-z0-9_-]+$", first.Token);
        Assert.True(first.Token.Length >= 43);
    }

    [Fact]
    public void RefreshToken_ExpiresAfterTheConfiguredDays()
    {
        RefreshToken token = _service.CreateRefreshToken();

        Assert.Equal(_time.Now.UtcDateTime.AddDays(30), token.TimeExpires);
    }

    [Fact]
    public void RefreshToken_HashMatchesHashRefreshToken_AndIsNotTheToken()
    {
        RefreshToken token = _service.CreateRefreshToken();

        Assert.Equal(_service.HashRefreshToken(token.Token), token.Hash);
        Assert.NotEqual(token.Token, token.Hash);
    }

    [Fact]
    public void HashRefreshToken_IsDeterministicHex()
    {
        string hash = _service.HashRefreshToken("token");

        Assert.Equal(hash, _service.HashRefreshToken("token"));
        Assert.NotEqual(hash, _service.HashRefreshToken("Token"));
        Assert.Matches("^[0-9A-F]{64}$", hash);
    }

    #endregion -- Refresh token --
}
