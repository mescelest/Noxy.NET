namespace Noxy.NET.MediaPlayer.Presentation;

public static class Constants
{
    // The key the refresh token is kept under in the platform's secure storage.
    public const string RefreshTokenKey = "refreshToken";

    // An access token that expires within this many seconds is refreshed before it is used.
    public const int RefreshMarginSeconds = 60;
}
