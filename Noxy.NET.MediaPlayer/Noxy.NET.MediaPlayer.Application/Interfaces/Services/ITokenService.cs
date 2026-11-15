using Noxy.NET.MediaPlayer.Application.Models.Authentication;
using Noxy.NET.MediaPlayer.Domain.Entities;
using RefreshToken = Noxy.NET.MediaPlayer.Application.Models.Authentication.RefreshToken;

namespace Noxy.NET.MediaPlayer.Application.Interfaces.Services;

public interface ITokenService
{
    AccessToken CreateAccessToken(User user);
    RefreshToken CreateRefreshToken();
    string HashRefreshToken(string refreshToken);
}
