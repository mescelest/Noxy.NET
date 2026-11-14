using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Interfaces.Repositories;
using Noxy.NET.MediaPlayer.Persistence.Interfaces.Mappers;
using Noxy.NET.MediaPlayer.Persistence.Repositories;

namespace Noxy.NET.MediaPlayer.Persistence;

public sealed class UnitOfWork(DataContext context, IEntityToTableMapper mapperE2T, ITableToEntityMapper mapperT2E) : IUnitOfWork
{
    public IAuthenticationRepository Authentication { get; } = new AuthenticationRepository(context, mapperE2T, mapperT2E);
    public IMediaRepository Media { get; } = new MediaRepository(context, mapperE2T, mapperT2E);
    public IPlaylistRepository Playlist { get; } = new PlaylistRepository(context, mapperE2T, mapperT2E);

    public async Task Commit(CancellationToken cancellationToken = default)
    {
        await context.SaveChangesAsync(cancellationToken);
    }

    public ValueTask DisposeAsync()
    {
        return context.DisposeAsync();
    }
}
