using Noxy.NET.MediaPlayer.Presentation.Enums;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeSessionService : ISessionService
{
    public string? AccessToken { get; set; } = "token-1";

    // What a successful refresh changes the access token to.
    public string NextAccessToken { get; set; } = "token-2";

    public bool RefreshSucceeds { get; set; } = true;

    public bool RestoreSucceeds { get; set; } = true;

    // Thrown by TryRestore when set.
    public Exception? RestoreException { get; set; }

    public int RefreshCount { get; private set; }

    public bool IsSignedIn => AccessToken is not null;

    public event Action? Changed;

    public void RaiseChanged()
    {
        Changed?.Invoke();
    }

    public SignInMethodEnum? SignInMethod { get; set; }

    public Task<SignInMethodEnum?> GetSignInMethod()
    {
        return Task.FromResult(SignInMethod);
    }

    public Task<string?> GetAccessToken()
    {
        return Task.FromResult(AccessToken);
    }

    public Task<bool> Refresh()
    {
        RefreshCount++;
        AccessToken = RefreshSucceeds ? NextAccessToken : null;

        return Task.FromResult(RefreshSucceeds);
    }

    public Task SignUp(string email, string password)
    {
        throw new NotSupportedException();
    }

    public Task SignIn(string email, string password)
    {
        throw new NotSupportedException();
    }

    public Task<bool> SignInWithGoogle(CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    // What TryRestore answers when set; otherwise SignedIn when RestoreSucceeds, else NoSession.
    public RestoreResultEnum? RestoreResult { get; set; }

    public int RestoreCount { get; private set; }

    public bool HasSession => IsSignedIn || RestoreResult == RestoreResultEnum.Offline;

    public Task<RestoreResultEnum> TryRestore()
    {
        if (RestoreException is not null) throw RestoreException;

        RestoreCount++;
        return Task.FromResult(RestoreResult ?? (RestoreSucceeds ? RestoreResultEnum.SignedIn : RestoreResultEnum.NoSession));
    }

    public Task SignOut()
    {
        AccessToken = null;
        return Task.CompletedTask;
    }
}
