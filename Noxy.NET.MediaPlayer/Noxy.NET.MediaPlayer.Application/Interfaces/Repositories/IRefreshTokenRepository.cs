using Noxy.NET.MediaPlayer.Domain.Entities;

namespace Noxy.NET.MediaPlayer.Application.Interfaces.Repositories;

public interface IRefreshTokenRepository : IRepository
{
    Task<RefreshToken?> FindByHash(string tokenHash, CancellationToken cancellationToken = default);
    void Add(RefreshToken refreshToken);

    // Both take effect immediately, not on Commit, so a revocation survives the exception that follows it.

    // Returns false if the token was already revoked, so only one of two concurrent uses can succeed.
    Task<bool> TryRevoke(Guid id, DateTime time, CancellationToken cancellationToken = default);

    Task RevokeAllForUser(Guid userID, DateTime time, CancellationToken cancellationToken = default);
}
