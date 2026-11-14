using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Interfaces.Repositories;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeUnitOfWork : IUnitOfWork
{
    private readonly FakeAuthenticationRepository _repository;
    private readonly FakeStore _store;

    public FakeUnitOfWork(FakeStore store)
    {
        _store = store;
        _repository = new(store);
    }

    public IAuthenticationRepository Authentication => _repository;

    public Task Commit(CancellationToken cancellationToken = default)
    {
        _repository.Flush();
        _store.CommitCount++;
        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}
