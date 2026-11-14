using System.Collections.Concurrent;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;

namespace Noxy.NET.MediaPlayer.Application.Services;

public sealed class UploadGate : IUploadGate
{
    private readonly ConcurrentDictionary<Guid, object> _running = new();

    public IDisposable? TryEnter(Guid userID)
    {
        object owner = new();
        if (!_running.TryAdd(userID, owner)) return null;

        return new UploadGateLease(() => _running.TryRemove(new KeyValuePair<Guid, object>(userID, owner)));
    }
}
