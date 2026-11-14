using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Enums;

namespace Noxy.NET.MediaPlayer.Application.Interfaces.Repositories;

public interface IAuthenticationRepository
{
    Task<bool> EmailExists(string email, CancellationToken cancellationToken = default);
    Task<User?> FindUserByID(Guid id, CancellationToken cancellationToken = default);
    Task<User?> FindUserByEmail(string email, CancellationToken cancellationToken = default);
    void AddUser(User user);

    Task<ExternalLogin?> FindExternalLogin(ExternalLoginProvider provider, string providerKey, CancellationToken cancellationToken = default);
    void AddExternalLogin(ExternalLogin externalLogin);

    Task<RefreshToken?> FindRefreshToken(string tokenHash, CancellationToken cancellationToken = default);
    void AddRefreshToken(RefreshToken refreshToken);
    Task<bool> TryRevokeRefreshToken(Guid id, DateTime time, CancellationToken cancellationToken = default);
    Task RevokeAllRefreshTokens(Guid userID, DateTime time, CancellationToken cancellationToken = default);

    // Deletes tokens that expired before the given time, whether or not they were revoked. Returns how many were deleted.
    Task<int> DeleteExpiredRefreshTokens(DateTime expiredBefore, CancellationToken cancellationToken = default);
}
