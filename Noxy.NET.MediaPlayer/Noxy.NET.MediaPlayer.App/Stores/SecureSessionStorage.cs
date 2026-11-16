using Noxy.NET.MediaPlayer.Presentation;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;

namespace Noxy.NET.MediaPlayer.App.Stores;

public class SecureSessionStorage : ISessionStorage
{
    public async Task<string?> GetRefreshToken()
    {
        try
        {
            return await SecureStorage.Default.GetAsync(Constants.RefreshTokenKey);
        }
        catch (Exception)
        {
            // The secure store can be unreadable (for example after the OS key changed): the user signs in again.
            SecureStorage.Default.Remove(Constants.RefreshTokenKey);
            return null;
        }
    }

    public async Task SetRefreshToken(string token)
    {
        await SecureStorage.Default.SetAsync(Constants.RefreshTokenKey, token);
    }

    public Task ClearRefreshToken()
    {
        SecureStorage.Default.Remove(Constants.RefreshTokenKey);
        return Task.CompletedTask;
    }
}
