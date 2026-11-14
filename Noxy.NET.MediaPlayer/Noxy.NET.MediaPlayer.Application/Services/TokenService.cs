using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Noxy.NET.MediaPlayer.Application.Configurations;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Application.Models;
using Noxy.NET.MediaPlayer.Domain.Entities;
using RefreshToken = Noxy.NET.MediaPlayer.Application.Models.RefreshToken;

namespace Noxy.NET.MediaPlayer.Application.Services;

public class TokenService(IOptions<ConfigJwt> options, TimeProvider time) : ITokenService
{
    private ConfigJwt Config { get; } = options.Value;
    private JsonWebTokenHandler Handler { get; } = new();

    public AccessToken CreateAccessToken(User user)
    {
        DateTime now = time.GetUtcNow().UtcDateTime;
        DateTime expires = now.AddMinutes(Config.AccessTokenMinutes);

        SecurityTokenDescriptor descriptor = new()
        {
            Subject = new(
            [
                new(JwtRegisteredClaimNames.Sub, user.ID.ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email),
                new(JwtRegisteredClaimNames.Jti, Guid.CreateVersion7().ToString())
            ]),
            Issuer = Config.Issuer,
            Audience = Config.Audience,
            NotBefore = now,
            Expires = expires,
            SigningCredentials = new(Config.CreateSigningKey(), SecurityAlgorithms.HmacSha256)
        };

        return new(Handler.CreateToken(descriptor), expires);
    }

    public RefreshToken CreateRefreshToken()
    {
        string token = Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(32));
        DateTime expires = time.GetUtcNow().UtcDateTime.AddDays(Config.RefreshTokenDays);

        return new(token, HashRefreshToken(token), expires);
    }

    public string HashRefreshToken(string refreshToken)
    {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));
    }
}
