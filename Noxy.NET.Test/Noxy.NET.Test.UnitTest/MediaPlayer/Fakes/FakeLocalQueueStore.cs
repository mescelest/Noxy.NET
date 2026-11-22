using System.Text.Json;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;
using Noxy.NET.MediaPlayer.Presentation.Models;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeLocalQueueStore : ILocalQueueStore
{
    private string? _json;

    // How many times Save was called.
    public int SaveCount { get; private set; }

    public Task<LocalQueueSnapshot?> Get()
    {
        return Task.FromResult(_json is null ? null : JsonSerializer.Deserialize<LocalQueueSnapshot>(_json, JsonSerializerOptions.Web));
    }

    public Task Save(LocalQueueSnapshot snapshot)
    {
        _json = JsonSerializer.Serialize(snapshot, JsonSerializerOptions.Web);
        SaveCount++;
        return Task.CompletedTask;
    }

    // How many times Clear was called.
    public int ClearCount { get; private set; }

    public Task Clear()
    {
        ClearCount++;
        _json = null;
        return Task.CompletedTask;
    }
}
