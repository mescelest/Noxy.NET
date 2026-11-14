using Noxy.NET.MediaPlayer.Application.Interfaces.Repositories;

namespace Noxy.NET.MediaPlayer.Application.Interfaces;

public interface IUnitOfWork : IAsyncDisposable
{
    IAuthenticationRepository Authentication { get; }

    Task Commit(CancellationToken cancellationToken = default);
}
