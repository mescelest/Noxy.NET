using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeSessionStorage : ISessionStorage
{
    public string? RefreshToken { get; set; }

    public Task<string?> GetRefreshToken()
    {
        return Task.FromResult(RefreshToken);
    }

    public Task SetRefreshToken(string token)
    {
        RefreshToken = token;
        return Task.CompletedTask;
    }

    public Task ClearRefreshToken()
    {
        RefreshToken = null;
        return Task.CompletedTask;
    }
}
