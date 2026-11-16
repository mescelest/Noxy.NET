using Noxy.NET.MediaPlayer.Application.Interfaces;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeUnitOfWorkFactory(FakeStore store) : IUnitOfWorkFactory
{
    // Runs when a unit of work is created, to let a test change the world while a handler waits on the database.
    public Action? OnCreate { get; set; }

    public IUnitOfWork Create()
    {
        OnCreate?.Invoke();
        return new FakeUnitOfWork(store);
    }
}
