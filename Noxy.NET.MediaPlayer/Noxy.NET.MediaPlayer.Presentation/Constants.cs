namespace Noxy.NET.MediaPlayer.Presentation;

public static class Constants
{
    // The key the refresh token is kept under in the platform's secure storage.
    public const string RefreshTokenKey = "refreshToken";

    // The key the server address is kept under in the platform's preferences.
    public const string ServerAddressKey = "serverAddress";

    // An access token that expires within this many seconds is refreshed before it is used.
    public const int RefreshMarginSeconds = 60;

    // Downloads that run at the same time; the rest wait for a free place.
    public const int MaxParallelDownloads = 3;

    // Tries for one download (a hash mismatch or a transient failure each use one) before it ends Failed.
    public const int MaxDownloadAttempts = 3;

    // The wait before the second try; it doubles for each try after that.
    public const int RetryBaseDelaySeconds = 2;

    // The player asks for the files of this many queue entries after the current one to be downloaded ahead of time.
    public const int PrefetchCount = 3;

    // A bulk download stops when the free space would not hold the next file plus this much.
    public const long StorageReserveBytes = 50L * 1024 * 1024;
}
