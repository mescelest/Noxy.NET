using Noxy.NET.MediaPlayer.Presentation;
using Noxy.NET.MediaPlayer.Presentation.Enums;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;

namespace Noxy.NET.MediaPlayer.App.Stores;

public class SecureSessionStorage : ISessionStorage
{
    public async Task<string?> GetRefreshToken()
    {
        try
        {
            return await SecureStorage.Default.GetAsync(PresentationConstants.RefreshTokenKey);
        }
        catch (Exception)
        {
            // The secure store can be unreadable (for example after the OS key changed): the user signs in again.
            SecureStorage.Default.Remove(PresentationConstants.RefreshTokenKey);
            return null;
        }
    }

    public async Task SetRefreshToken(string token)
    {
        await SecureStorage.Default.SetAsync(PresentationConstants.RefreshTokenKey, token);
    }

    public Task ClearRefreshToken()
    {
        SecureStorage.Default.Remove(PresentationConstants.RefreshTokenKey);
        return Task.CompletedTask;
    }

    public async Task<SignInMethodEnum?> GetSignInMethod()
    {
        try
        {
            string? text = await SecureStorage.Default.GetAsync(PresentationConstants.SignInMethodKey);

            return Enum.TryParse(text, out SignInMethodEnum method) && Enum.IsDefined(method) ? method : null;
        }
        catch (Exception)
        {
            // Only a note for the settings page: when it cannot be read it is not shown.
            SecureStorage.Default.Remove(PresentationConstants.SignInMethodKey);
            return null;
        }
    }

    public async Task SetSignInMethod(SignInMethodEnum method)
    {
        await SecureStorage.Default.SetAsync(PresentationConstants.SignInMethodKey, method.ToString());
    }

    public Task ClearSignInMethod()
    {
        SecureStorage.Default.Remove(PresentationConstants.SignInMethodKey);
        return Task.CompletedTask;
    }
}
