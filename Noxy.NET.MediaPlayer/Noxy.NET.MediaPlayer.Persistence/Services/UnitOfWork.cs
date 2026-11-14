using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Interfaces.Repositories;
using Noxy.NET.MediaPlayer.Persistence.Interfaces.Services;
using Noxy.NET.MediaPlayer.Persistence.Repositories;

namespace Noxy.NET.MediaPlayer.Persistence.Services;

public sealed class UnitOfWork(DataContext context, IEntityToTableMapper mapperE2T, ITableToEntityMapper mapperT2E) : IUnitOfWork
{
    public IAuthenticationRepository Authentication { get; } = new AuthenticationRepository(context, mapperE2T, mapperT2E);

    public async Task Commit(CancellationToken cancellationToken = default)
    {
        await context.SaveChangesAsync(cancellationToken);
    }

    public ValueTask DisposeAsync()
    {
        return context.DisposeAsync();
    }
}
