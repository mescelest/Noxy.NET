using Noxy.NET.MediaPlayer.Application.Interfaces;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeUnitOfWorkFactory(FakeStore store) : IUnitOfWorkFactory
{
    public IUnitOfWork Create()
    {
        return new FakeUnitOfWork(store);
    }
}
