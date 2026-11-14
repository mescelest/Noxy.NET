using Microsoft.EntityFrameworkCore;
using Noxy.NET.MediaPlayer.Application.Interfaces.Repositories;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Persistence.Mappers;
using Noxy.NET.MediaPlayer.Persistence.Tables.Authentication;

namespace Noxy.NET.MediaPlayer.Persistence.Repositories;

public class AuthenticationRepository(DataContext context) : IAuthenticationRepository
{
    public Task<bool> EmailExists(string email, CancellationToken cancellationToken = default)
    {
        return context.User.AnyAsync(x => x.Email == email, cancellationToken);
    }

    public async Task<User?> FindUserByID(Guid id, CancellationToken cancellationToken = default)
    {
        TableUser? table = await context.User.AsNoTracking().FirstOrDefaultAsync(x => x.ID == id, cancellationToken);
        return table?.ToEntity();
    }

    public async Task<User?> FindUserByEmail(string email, CancellationToken cancellationToken = default)
    {
        TableUser? table = await context.User.AsNoTracking().FirstOrDefaultAsync(x => x.Email == email, cancellationToken);
        return table?.ToEntity();
    }

    public void AddUser(User user)
    {
        context.User.Add(user.ToTable());
    }

    public async Task<ExternalLogin?> FindExternalLogin(ExternalLoginProvider provider, string providerKey, CancellationToken cancellationToken = default)
    {
        TableExternalLogin? table = await context.ExternalLogin.AsNoTracking().FirstOrDefaultAsync(x => x.Provider == provider && x.ProviderKey == providerKey, cancellationToken);
        return table?.ToEntity();
    }

    public void AddExternalLogin(ExternalLogin externalLogin)
    {
        context.ExternalLogin.Add(externalLogin.ToTable());
    }

    public async Task<RefreshToken?> FindRefreshToken(string tokenHash, CancellationToken cancellationToken = default)
    {
        TableRefreshToken? table = await context.RefreshToken.AsNoTracking().FirstOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);
        return table?.ToEntity();
    }

    public void AddRefreshToken(RefreshToken refreshToken)
    {
        context.RefreshToken.Add(refreshToken.ToTable());
    }

    public async Task<bool> TryRevokeRefreshToken(Guid id, DateTime time, CancellationToken cancellationToken = default)
    {
        int affected = await context.RefreshToken
            .Where(x => x.ID == id && x.TimeRevoked == null)
            .ExecuteUpdateAsync(x => x.SetProperty(e => e.TimeRevoked, (DateTime?)time), cancellationToken);

        return affected == 1;
    }

    public async Task RevokeAllRefreshTokens(Guid userID, DateTime time, CancellationToken cancellationToken = default)
    {
        await context.RefreshToken
            .Where(x => x.UserID == userID && x.TimeRevoked == null)
            .ExecuteUpdateAsync(x => x.SetProperty(e => e.TimeRevoked, (DateTime?)time), cancellationToken);
    }

    public Task<int> DeleteExpiredRefreshTokens(DateTime expiredBefore, CancellationToken cancellationToken = default)
    {
        return context.RefreshToken
            .Where(x => x.TimeExpires < expiredBefore)
            .ExecuteDeleteAsync(cancellationToken);
    }
}
