using Noxy.NET.MediaPlayer.Application.Interfaces.Repositories;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Enums;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeAuthenticationRepository(FakeStore store) : IAuthenticationRepository
{
    private readonly List<EntityExternalLogin> _pendingExternalLogins = [];
    private readonly List<EntityRefreshToken> _pendingRefreshTokens = [];
    private readonly List<EntityUser> _pendingUsers = [];

    public void Flush()
    {
        store.UserList.AddRange(_pendingUsers);
        store.ExternalLoginList.AddRange(_pendingUsers.SelectMany(x => x.ExternalLoginList));
        store.ExternalLoginList.AddRange(_pendingExternalLogins);
        store.RefreshTokenList.AddRange(_pendingRefreshTokens);

        _pendingUsers.Clear();
        _pendingExternalLogins.Clear();
        _pendingRefreshTokens.Clear();
    }

    public Task<bool> EmailExists(string email, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(store.UserList.Any(x => x.Email == email));
    }

    public Task<EntityUser?> FindUserByID(Guid id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(store.UserList.FirstOrDefault(x => x.ID == id));
    }

    public Task<EntityUser?> FindUserByEmail(string email, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(store.UserList.FirstOrDefault(x => x.Email == email));
    }

    public void AddUser(EntityUser user)
    {
        _pendingUsers.Add(user);
    }

    public Task<EntityExternalLogin?> FindExternalLogin(ExternalLoginProviderEnum provider, string providerKey, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(store.ExternalLoginList.FirstOrDefault(x => x.Provider == provider && x.ProviderKey == providerKey));
    }

    public void AddExternalLogin(EntityExternalLogin externalLogin)
    {
        _pendingExternalLogins.Add(externalLogin);
    }

    public Task<EntityRefreshToken?> FindRefreshToken(string tokenHash, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(store.RefreshTokenList.FirstOrDefault(x => x.TokenHash == tokenHash));
    }

    public void AddRefreshToken(EntityRefreshToken refreshToken)
    {
        _pendingRefreshTokens.Add(refreshToken);
    }

    public Task<bool> TryRevokeRefreshToken(Guid id, DateTime time, CancellationToken cancellationToken = default)
    {
        EntityRefreshToken? token = store.RefreshTokenList.FirstOrDefault(x => x.ID == id && !x.IsRevoked);
        if (token is null || store.LoseRevokeRace) return Task.FromResult(false);

        token.TimeRevoked = time;
        return Task.FromResult(true);
    }

    public Task RevokeAllRefreshTokens(Guid userID, DateTime time, CancellationToken cancellationToken = default)
    {
        store.RevokeAllCount++;
        foreach (EntityRefreshToken token in store.RefreshTokenList.Where(x => x.UserID == userID && !x.IsRevoked))
        {
            token.TimeRevoked = time;
        }

        return Task.CompletedTask;
    }

    public Task<int> DeleteExpiredRefreshTokens(DateTime expiredBefore, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(store.RefreshTokenList.RemoveAll(x => x.TimeExpires < expiredBefore));
    }
}
