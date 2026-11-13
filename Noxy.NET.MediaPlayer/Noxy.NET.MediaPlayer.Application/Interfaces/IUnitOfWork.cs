using Noxy.NET.MediaPlayer.Application.Interfaces.Repositories;

namespace Noxy.NET.MediaPlayer.Application.Interfaces;

public interface IUnitOfWork : IAsyncDisposable
{
    IUserRepository Users { get; }
    IRefreshTokenRepository RefreshTokens { get; }
    IExternalLoginRepository ExternalLogins { get; }

    Task Commit(CancellationToken cancellationToken = default);
}
