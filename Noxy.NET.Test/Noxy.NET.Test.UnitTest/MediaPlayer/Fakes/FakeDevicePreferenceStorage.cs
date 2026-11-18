using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeDevicePreferenceStorage : IDevicePreferenceStorage
{
    private readonly Dictionary<string, string> _map = [];

    // Thrown by Get, Set and Remove when set, as a platform store that cannot be reached would.
    public Exception? Failure { get; set; }

    public Task<string?> Get(string key)
    {
        if (Failure is not null) throw Failure;

        return Task.FromResult(_map.TryGetValue(key, out string? value) ? value : null);
    }

    public Task Set(string key, string value)
    {
        if (Failure is not null) throw Failure;

        _map[key] = value;
        return Task.CompletedTask;
    }

    public Task Remove(string key)
    {
        if (Failure is not null) throw Failure;

        _map.Remove(key);
        return Task.CompletedTask;
    }
}
