using Noxy.NET.MediaPlayer.Application.Models;
using Noxy.NET.MediaPlayer.Domain.Entities;
using RefreshToken = Noxy.NET.MediaPlayer.Application.Models.RefreshToken;

namespace Noxy.NET.MediaPlayer.Application.Interfaces.Services;

public interface ITokenService
{
    AccessToken CreateAccessToken(User user);
    RefreshToken CreateRefreshToken();
    string HashRefreshToken(string refreshToken);
}
