using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeGoogleSignInService : IGoogleSignInService
{
    // What the user "signs in with". Null is a user who closed the browser or denied access.
    public string? IdToken { get; set; } = "google-id-token";

    public int CallCount { get; private set; }

    public Task<string?> GetIdToken(CancellationToken cancellationToken)
    {
        CallCount++;
        return Task.FromResult(IdToken);
    }
}
