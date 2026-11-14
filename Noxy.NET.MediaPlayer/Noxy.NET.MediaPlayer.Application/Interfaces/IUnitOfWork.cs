using Noxy.NET.MediaPlayer.Application.Interfaces.Repositories;

namespace Noxy.NET.MediaPlayer.Application.Interfaces;

public interface IUnitOfWork : IAsyncDisposable
{
    IAuthenticationRepository Authentication { get; }
    IMediaRepository Media { get; }
    IPlaylistRepository Playlist { get; }

    Task Commit(CancellationToken cancellationToken = default);
}
