using Noxy.NET.MediaPlayer.Domain.Entities;

namespace Noxy.NET.MediaPlayer.Application.Interfaces.Repositories;

public interface IRefreshTokenRepository : IRepository
{
    Task<RefreshToken?> FindByHash(string tokenHash, CancellationToken cancellationToken = default);
    void Add(RefreshToken refreshToken);
}
