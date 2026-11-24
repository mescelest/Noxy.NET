using Noxy.NET.MediaPlayer.Presentation.Enums;

namespace Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;

public interface ISessionStorage
{
    Task<string?> GetRefreshToken();

    Task SetRefreshToken(string token);

    Task ClearRefreshToken();

    // How the user signed in on this device; null when it was not saved (a session from before it was kept).
    Task<SignInMethodEnum?> GetSignInMethod();

    Task SetSignInMethod(SignInMethodEnum method);

    Task ClearSignInMethod();
}
