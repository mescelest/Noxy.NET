using Noxy.NET.MediaPlayer.Application.Models.Authentication;
using Noxy.NET.MediaPlayer.Domain.Entities;

namespace Noxy.NET.MediaPlayer.Application.Interfaces.Services;

public interface ITokenService
{
    AccessToken CreateAccessToken(EntityUser user);
    RefreshToken CreateRefreshToken();
    string HashRefreshToken(string refreshToken);
}
