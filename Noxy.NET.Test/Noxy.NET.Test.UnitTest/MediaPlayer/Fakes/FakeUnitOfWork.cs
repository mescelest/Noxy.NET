using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Interfaces.Repositories;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeUnitOfWork(FakeStore store) : IUnitOfWork
{
    private readonly FakeAuthenticationRepository _repository = new(store);
    private readonly FakeMediaRepository _media = new(store);
    private readonly FakePlaylistRepository _playlist = new(store);

    public IAuthenticationRepository Authentication => _repository;
    public IMediaRepository Media => _media;
    public IPlaylistRepository Playlist => _playlist;

    public Task Commit(CancellationToken cancellationToken = default)
    {
        if (store.FailCommit) throw new InvalidOperationException("The commit failed.");

        if (store.FailCommitCount > 0)
        {
            store.FailCommitCount--;
            throw new InvalidOperationException("The commit failed.");
        }

        _repository.Flush();
        _media.Flush();
        _playlist.Flush();
        store.CommitCount++;
        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}
