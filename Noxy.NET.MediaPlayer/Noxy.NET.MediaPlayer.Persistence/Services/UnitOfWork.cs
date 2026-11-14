using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Interfaces.Repositories;
using Noxy.NET.MediaPlayer.Persistence.Repositories;

namespace Noxy.NET.MediaPlayer.Persistence.Services;

public sealed class UnitOfWork(DataContext context) : IUnitOfWork
{
    public IAuthenticationRepository Authentication { get; } = new AuthenticationRepository(context);

    public async Task Commit(CancellationToken cancellationToken = default)
    {
        await context.SaveChangesAsync(cancellationToken);
    }

    public ValueTask DisposeAsync()
    {
        return context.DisposeAsync();
    }
}
