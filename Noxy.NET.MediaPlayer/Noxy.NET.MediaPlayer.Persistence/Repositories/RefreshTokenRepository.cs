using Microsoft.EntityFrameworkCore;
using Noxy.NET.MediaPlayer.Application.Interfaces.Repositories;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Persistence.Mappers;
using Noxy.NET.MediaPlayer.Persistence.Tables.Authentication;

namespace Noxy.NET.MediaPlayer.Persistence.Repositories;

public class RefreshTokenRepository(DataContext context) : IRefreshTokenRepository
{
    public async Task<RefreshToken?> FindByHash(string tokenHash, CancellationToken cancellationToken = default)
    {
        TableRefreshToken? table = await context.RefreshToken.AsNoTracking().FirstOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);
        return table?.ToEntity();
    }

    public void Add(RefreshToken refreshToken)
    {
        context.RefreshToken.Add(refreshToken.ToTable());
    }

    public async Task<bool> TryRevoke(Guid id, DateTime time, CancellationToken cancellationToken = default)
    {
        int affected = await context.RefreshToken
            .Where(x => x.ID == id && x.TimeRevoked == null)
            .ExecuteUpdateAsync(x => x.SetProperty(e => e.TimeRevoked, (DateTime?)time), cancellationToken);

        return affected == 1;
    }

    public async Task RevokeAllForUser(Guid userID, DateTime time, CancellationToken cancellationToken = default)
    {
        await context.RefreshToken
            .Where(x => x.UserID == userID && x.TimeRevoked == null)
            .ExecuteUpdateAsync(x => x.SetProperty(e => e.TimeRevoked, (DateTime?)time), cancellationToken);
    }
}
