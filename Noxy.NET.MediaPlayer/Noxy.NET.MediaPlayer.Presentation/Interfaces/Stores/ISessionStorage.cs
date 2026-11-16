namespace Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;

public interface ISessionStorage
{
    Task<string?> GetRefreshToken();

    Task SetRefreshToken(string token);

    Task ClearRefreshToken();
}
