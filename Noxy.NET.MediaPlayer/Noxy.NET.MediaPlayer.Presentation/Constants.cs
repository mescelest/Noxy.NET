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

    // A seek or a volume change made on a controller is sent this long after the last change of a burst.
    public const int RemoteDeferMilliseconds = 300;

    // While playing, the local copy of the queue saves its position at most this often.
    public const int LocalQueueSaveSeconds = 10;

    // The most items the played history of the local queue holds.
    public const int QueueHistoryLimit = 50;

    // A controller shows the active device's position as the last report plus the time since; a new report that is behind that by no more than this is a delay, not a seek back, and the shown position stays.
    public const double RemotePositionToleranceSeconds = 2;

    // The longest a device name may be.
    public const int DeviceNameMaxLength = 50;
}
