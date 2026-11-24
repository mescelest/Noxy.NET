using Noxy.NET.MediaPlayer.Presentation.Enums;
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

    public SignInMethodEnum? SignInMethod { get; set; }

    public Task<SignInMethodEnum?> GetSignInMethod()
    {
        return Task.FromResult(SignInMethod);
    }

    public Task SetSignInMethod(SignInMethodEnum method)
    {
        SignInMethod = method;
        return Task.CompletedTask;
    }

    public Task ClearSignInMethod()
    {
        SignInMethod = null;
        return Task.CompletedTask;
    }
}
