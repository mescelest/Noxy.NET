using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Interfaces.Repositories;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeUnitOfWork : IUnitOfWork
{
    private readonly FakeAuthenticationRepository _repository;
    private readonly FakeMediaRepository _media;
    private readonly FakeStore _store;

    public FakeUnitOfWork(FakeStore store)
    {
        _store = store;
        _repository = new(store);
        _media = new(store);
    }

    public IAuthenticationRepository Authentication => _repository;
    public IMediaRepository Media => _media;

    public Task Commit(CancellationToken cancellationToken = default)
    {
        if (_store.FailCommit) throw new InvalidOperationException("The commit failed.");

        if (_store.FailCommitCount > 0)
        {
            _store.FailCommitCount--;
            throw new InvalidOperationException("The commit failed.");
        }

        _repository.Flush();
        _media.Flush();
        _store.CommitCount++;
        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}
