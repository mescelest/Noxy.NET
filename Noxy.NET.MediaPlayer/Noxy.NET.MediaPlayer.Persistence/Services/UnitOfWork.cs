using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Interfaces.Repositories;
using Noxy.NET.MediaPlayer.Persistence.Repositories;

namespace Noxy.NET.MediaPlayer.Persistence.Services;

public sealed class UnitOfWork(DataContext context) : IUnitOfWork
{
    public IUserRepository Users { get; } = new UserRepository(context);
    public IRefreshTokenRepository RefreshTokens { get; } = new RefreshTokenRepository(context);
    public IExternalLoginRepository ExternalLogins { get; } = new ExternalLoginRepository(context);

    public async Task Commit(CancellationToken cancellationToken = default)
    {
        await context.SaveChangesAsync(cancellationToken);
    }

    public ValueTask DisposeAsync()
    {
        return context.DisposeAsync();
    }
}
