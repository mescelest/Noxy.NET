namespace Noxy.NET.MediaPlayer.Presentation;

public static class PresentationConstants
{
    // The key the refresh token is kept under in the platform's secure storage.
    public const string RefreshTokenKey = "refreshToken";

    // The key the way the user signed in is kept under in the platform's secure storage.
    public const string SignInMethodKey = "signInMethod";

    // The key the server address is kept under in the platform's preferences.
    public const string ServerAddressKey = "serverAddress";
    public const string ServerAddressListKey = "serverAddressList";

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

    // The wait before the second try; it doubles for each try after that.
    public const int RetryBaseDelaySeconds = 2;

    // A bulk download stops when the free space would not hold the next file plus this much.
    public const long StorageReserveBytes = 50L * 1024 * 1024;

    // A seek or a volume change made on a controller is sent this long after the last change of a burst.
    public const int RemoteDeferMilliseconds = 300;

    // A shuffle or repeat mode changes on screen at once and is sent to the server this long after the last change.
    public const int ModeSaveMilliseconds = 500;

    // The most items a source puts into the queue: the same as the most media files a library may hold (the server default), so a whole library fits. It keeps the saved queue and what is sent to the server small.
    public const int QueueMaxItemCount = 5000;

    // The most items the played history of the local queue holds.
    public const int QueueHistoryLimit = 50;

    // A controller shows the active device's position as the last report plus the time since; a new report that is behind that by no more than this is a delay, not a seek back, and the shown position stays.
    public const double RemotePositionToleranceSeconds = 2;

    // The longest a device name may be.
    public const int DeviceNameMaxLength = 50;
}
