using Noxy.NET.MediaPlayer.Application.Interfaces.Repositories;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Enums;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeAuthenticationRepository(FakeStore store) : IAuthenticationRepository
{
    private readonly List<ExternalLogin> _pendingExternalLogins = [];
    private readonly List<RefreshToken> _pendingRefreshTokens = [];
    private readonly List<User> _pendingUsers = [];

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

    public Task<User?> FindUserByID(Guid id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(store.UserList.FirstOrDefault(x => x.ID == id));
    }

    public Task<User?> FindUserByEmail(string email, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(store.UserList.FirstOrDefault(x => x.Email == email));
    }

    public void AddUser(User user)
    {
        _pendingUsers.Add(user);
    }

    public Task<ExternalLogin?> FindExternalLogin(ExternalLoginProvider provider, string providerKey, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(store.ExternalLoginList.FirstOrDefault(x => x.Provider == provider && x.ProviderKey == providerKey));
    }

    public void AddExternalLogin(ExternalLogin externalLogin)
    {
        _pendingExternalLogins.Add(externalLogin);
    }

    public Task<RefreshToken?> FindRefreshToken(string tokenHash, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(store.RefreshTokenList.FirstOrDefault(x => x.TokenHash == tokenHash));
    }

    public void AddRefreshToken(RefreshToken refreshToken)
    {
        _pendingRefreshTokens.Add(refreshToken);
    }

    public Task<bool> TryRevokeRefreshToken(Guid id, DateTime time, CancellationToken cancellationToken = default)
    {
        RefreshToken? token = store.RefreshTokenList.FirstOrDefault(x => x.ID == id && !x.IsRevoked);
        if (token == null || store.LoseRevokeRace) return Task.FromResult(false);

        token.TimeRevoked = time;
        return Task.FromResult(true);
    }

    public Task RevokeAllRefreshTokens(Guid userID, DateTime time, CancellationToken cancellationToken = default)
    {
        store.RevokeAllCount++;
        foreach (RefreshToken token in store.RefreshTokenList.Where(x => x.UserID == userID && !x.IsRevoked))
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
