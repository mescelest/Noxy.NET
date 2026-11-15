using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Application.Models.Authentication;
using Noxy.NET.MediaPlayer.Domain.Entities;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeTokenService(TimeProvider time) : ITokenService
{
    private int _counter;

    public AccessToken CreateAccessToken(EntityUser user)
    {
        return new($"access-for-{user.Email}", time.GetUtcNow().UtcDateTime.AddMinutes(15));
    }

    public RefreshToken CreateRefreshToken()
    {
        string token = $"refresh-{++_counter}";
        return new(token, HashRefreshToken(token), time.GetUtcNow().UtcDateTime.AddDays(30));
    }

    public string HashRefreshToken(string refreshToken)
    {
        return $"hash-of-{refreshToken}";
    }
}
