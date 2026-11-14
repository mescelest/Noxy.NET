using System.Collections.Concurrent;
using Noxy.NET.MediaPlayer.Application.Interfaces.Gates;

namespace Noxy.NET.MediaPlayer.Application.Gates;

public sealed class UploadGate : IUploadGate
{
    private readonly ConcurrentDictionary<Guid, object> _running = new();

    public IDisposable? TryEnter(Guid userID)
    {
        object owner = new();
        return !_running.TryAdd(userID, owner) ? null : new UploadGateLease(() => _running.TryRemove(new(userID, owner)));
    }
}
