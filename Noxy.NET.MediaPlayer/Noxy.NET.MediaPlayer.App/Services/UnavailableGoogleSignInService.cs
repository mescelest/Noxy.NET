using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;

namespace Noxy.NET.MediaPlayer.App.Services;

// Stands in until the real Google sign-in is built (Task 11): as far as the session is concerned, the user always cancels.
public class UnavailableGoogleSignInService : IGoogleSignInService
{
    public Task<string?> GetIdToken(CancellationToken cancellationToken)
    {
        return Task.FromResult<string?>(null);
    }
}
