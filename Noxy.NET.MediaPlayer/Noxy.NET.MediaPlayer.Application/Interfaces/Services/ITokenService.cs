using Noxy.NET.MediaPlayer.Application.Models;
using Noxy.NET.MediaPlayer.Domain.Entities;

namespace Noxy.NET.MediaPlayer.Application.Interfaces.Services;

public interface ITokenService
{
    AccessToken CreateAccessToken(User user);
    RefreshTokenSecret CreateRefreshToken();
    string HashRefreshToken(string refreshToken);
}
