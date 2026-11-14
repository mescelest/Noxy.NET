using System.Collections.Concurrent;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeUploadProcessor : IUploadProcessor
{
    private int _processNextCount;

    // Called with the zero-based call number: return true while there is work, false when idle.
    public Func<int, Task<bool>> ProcessNextBehavior { get; set; } = _ => Task.FromResult(false);
    public Func<Task> RecoverBehavior { get; set; } = () => Task.CompletedTask;

    public ConcurrentQueue<string> Calls { get; } = [];
    public int ProcessNextCount => Volatile.Read(ref _processNextCount);

    public async Task<bool> ProcessNext(CancellationToken cancellationToken = default)
    {
        Calls.Enqueue("ProcessNext");
        return await ProcessNextBehavior(Interlocked.Increment(ref _processNextCount) - 1);
    }

    public async Task Recover(CancellationToken cancellationToken = default)
    {
        Calls.Enqueue("Recover");
        await RecoverBehavior();
    }
}
