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

    // With a stored session and no network, start-up tries again after this many seconds, then twice as long each time, up to the maximum.
    public const int StartupRetryBaseSeconds = 5;

    public const int StartupRetryMaxSeconds = 60;

    // The keys of the small choices kept on this device (IDevicePreferenceStorage).
    public const string SidebarOpenKey = "sidebarOpen";

    // A call to the server that has not answered after this many seconds counts as not reached (the body of a file being streamed is not covered).
    public const int HttpTimeoutSeconds = 30;

    // How many playlist cards the Home page shows.
    public const int HomeRecentCount = 6;

    // What the sign-in form says when the server gave no message of its own.
    public const string SignInMessageOffline = "The server cannot be reached. Check the server address and your connection.";

    public const string SignInMessageNoAddress = "Enter the server address first.";

    public const string SignInMessageFailed = "Signing in failed. Try again.";

    // What the library actions say when the server gave no message of its own.
    public const string ActionMessageOffline = "This needs the server, and it cannot be reached right now.";

    public const string ActionMessageRemoveFailed = "Some files could not be removed from this device.";

    public const string ActionMessageDeleteFailed = "Some media files could not be deleted.";

    public const string ActionMessageAddFailed = "The media files could not be added to the playlist.";

    public const string ActionMessageSaveFailed = "The file could not be saved.";

    public const string ActionMessageFailed = "Something went wrong. Try again.";

    // The wait before the second try; it doubles for each try after that.
    public const int RetryBaseDelaySeconds = 2;

    // The player asks for the files of this many queue entries after the current one to be downloaded ahead of time.
    public const int PrefetchCount = 3;

    // A bulk download stops when the free space would not hold the next file plus this much.
    public const long StorageReserveBytes = 50L * 1024 * 1024;

    // A seek or a volume change made on a controller is sent this long after the last change of a burst.
    public const int RemoteDeferMilliseconds = 300;

    // The most items a source puts into the queue: the same as the most media files a library may hold (the server default), so a whole library fits. It keeps the saved queue and what is sent to the server small.
    public const int QueueMaxItemCount = 5000;

    // The most items the played history of the local queue holds.
    public const int QueueHistoryLimit = 50;

    // A controller shows the active device's position as the last report plus the time since; a new report that is behind that by no more than this is a delay, not a seek back, and the shown position stays.
    public const double RemotePositionToleranceSeconds = 2;

    // The name of a device whose machine has none.
    public const string DefaultDeviceName = "This device";

    // The longest a device name may be.
    public const int DeviceNameMaxLength = 50;
}
