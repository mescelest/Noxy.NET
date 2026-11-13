using Noxy.NET.MediaPlayer.Domain.Entities;

namespace Noxy.NET.MediaPlayer.Application.Interfaces.Repositories;

public interface IRefreshTokenRepository : IRepository
{
    Task<RefreshToken?> FindByHash(string tokenHash, CancellationToken cancellationToken = default);
    void Add(RefreshToken refreshToken);
    Task<bool> TryRevoke(Guid id, DateTime time, CancellationToken cancellationToken = default);
    Task RevokeAllForUser(Guid userID, DateTime time, CancellationToken cancellationToken = default);

    // Deletes tokens that expired before the given time, whether or not they were revoked. Returns how many were deleted.
    Task<int> DeleteExpired(DateTime expiredBefore, CancellationToken cancellationToken = default);
}
