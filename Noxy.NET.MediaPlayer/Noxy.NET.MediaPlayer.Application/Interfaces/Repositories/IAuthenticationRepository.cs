using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Enums;

namespace Noxy.NET.MediaPlayer.Application.Interfaces.Repositories;

public interface IAuthenticationRepository
{
    Task<bool> EmailExists(string email, CancellationToken cancellationToken = default);
    Task<EntityUser?> FindUserByID(Guid id, CancellationToken cancellationToken = default);
    Task<EntityUser?> FindUserByEmail(string email, CancellationToken cancellationToken = default);
    void AddUser(EntityUser user);

    Task<EntityExternalLogin?> FindExternalLogin(ExternalLoginProviderEnum provider, string providerKey, CancellationToken cancellationToken = default);
    void AddExternalLogin(EntityExternalLogin externalLogin);

    Task<EntityRefreshToken?> FindRefreshToken(string tokenHash, CancellationToken cancellationToken = default);
    void AddRefreshToken(EntityRefreshToken refreshToken);
    Task<bool> TryRevokeRefreshToken(Guid id, DateTime time, CancellationToken cancellationToken = default);
    Task RevokeAllRefreshTokens(Guid userID, DateTime time, CancellationToken cancellationToken = default);

    // Deletes tokens that expired before the given time, whether or not they were revoked. Returns how many were deleted.
    Task<int> DeleteExpiredRefreshTokens(DateTime expiredBefore, CancellationToken cancellationToken = default);
}
