using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;

namespace Noxy.NET.MediaPlayer.App.Services;

public class UnavailableGoogleSignInService : IGoogleSignInService
{
    public Task<string?> GetIdToken(CancellationToken cancellationToken)
    {
        return Task.FromResult<string?>(null);
    }
}
