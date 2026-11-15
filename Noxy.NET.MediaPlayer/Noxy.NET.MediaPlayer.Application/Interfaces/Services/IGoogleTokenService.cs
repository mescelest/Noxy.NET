using Noxy.NET.MediaPlayer.Application.Models.Authentication;

namespace Noxy.NET.MediaPlayer.Application.Interfaces.Services;

public interface IGoogleTokenService
{
    // Throws AuthenticationFailedException for any invalid token, and GoogleLoginNotConfiguredException if no client ID is set.
    Task<GoogleIdentity> Validate(string idToken);
}
