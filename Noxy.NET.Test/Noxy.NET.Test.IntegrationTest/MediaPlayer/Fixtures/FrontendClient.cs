using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;
using Noxy.NET.MediaPlayer.Presentation.Services;
using Noxy.NET.MediaPlayer.Storage;
using Noxy.NET.MediaPlayer.Storage.Stores;
using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Fixtures;

public sealed class FrontendClient : IAsyncDisposable
{
    private readonly bool _ownsFolder;

    private FrontendClient(ApiFactory factory, FakeSessionStorage sessionStorage, string folder, bool ownsFolder)
    {
        _ownsFolder = ownsFolder;
        Folder = folder;
        SessionStorage = sessionStorage;
        Time.Now = DateTimeOffset.UtcNow;

        Network = new(factory.Server.CreateHandler());
        HttpClient httpClient = new(Network, false);
        FakeServerAddressStorage storageServerAddress = new(factory.Server.BaseAddress.ToString().TrimEnd('/'));

        Store = new(Path.Combine(folder, StorageConstants.MetadataDatabaseFileName));
        Session = new(new AuthApiClient(httpClient, storageServerAddress), new FakeGoogleSignInService(), sessionStorage, Time);
        Api = new(httpClient, Session, storageServerAddress);
        Sync = new(Api, Store);
        SettingService = new(Api, Store.Setting);
        MetadataSync = new(Api, Store);
        Hub = new(Sync, SettingService, MetadataSync, Session, storageServerAddress, NullLogger<MediaHubService>.Instance, Network);

        FileStore = new(folder);
        Download = new(Api, FileStore, new HashService(), Store, new Delay(), NullLogger<DownloadService>.Instance);
        Reconciler = new(FileStore, Store, Download);

        QueueSource = new(Store, new ShuffleService(Random.Shared), Random.Shared);
        QueueFile = new(Path.Combine(folder, StorageConstants.LocalQueueFileName));
        Queue = new(QueueSource, SettingService, QueueFile, Sync, Store, Time, NullLogger<QueueService>.Instance);
        DeviceFile = new(Path.Combine(folder, StorageConstants.DeviceFileName));
        Device = new(DeviceFile);
        Player = new(Audio, Download, FileStore, Store, Sync, DeviceFile, Queue, QueueSource, SettingService, Api, NullLogger<PlayerService>.Instance);
        Remote = new(Hub, Device, Queue, Time, NullLogger<RemotePlaybackService>.Instance);
        Reporter = new(Player, Queue, Remote, Hub, Store, NullLogger<PlaybackReporterService>.Instance);
        CommandHandler = new(Hub, Remote, Player, Queue, Reporter, NullLogger<RemoteCommandHandlerService>.Instance);
        Control = new(Player, Queue, Remote, Hub, Reporter, Device, new Delay(), NullLogger<PlaybackControlService>.Instance);
        LocalDataOwner = new(Session, Store, Store, Store.Setting, FileStore, Download, Player);
        Startup = new(Session, Sync, SettingService, MetadataSync, Hub, Reconciler, Queue, Reporter, CommandHandler, LocalDataOwner, Player, new Delay(), NullLogger<AppStartupService>.Instance);
    }

    public FakeTimeProvider Time { get; } = new();

    public FakeNetworkHandler Network { get; }

    public FakeSessionStorage SessionStorage { get; }

    public string Folder { get; }

    public SqliteMetadataStore Store { get; }

    public SessionService Session { get; }

    public SyncService Sync { get; }

    public SettingService SettingService { get; }

    public MetadataSyncService MetadataSync { get; }

    public FakeServerMediaHubService Hub { get; }

    public AppStartupService Startup { get; }

    public ApiClient Api { get; }

    public DiskFileStore FileStore { get; }

    public DownloadService Download { get; }

    public FileReconciler Reconciler { get; }

    // No sound device in a test: the player is driven against a fake that records what it was asked.
    public FakeAudioPlayer Audio { get; } = new();

    public QueueSourceService QueueSource { get; }

    public FileLocalQueueStore QueueFile { get; }

    public QueueService Queue { get; }

    public PlayerService Player { get; }

    public FileDeviceStore DeviceFile { get; }

    public LocalDataOwnerService LocalDataOwner { get; }

    public DeviceService Device { get; }

    public RemotePlaybackService Remote { get; }

    public PlaybackReporterService Reporter { get; }

    public RemoteCommandHandlerService CommandHandler { get; }

    public PlaybackControlService Control { get; }

    // A folder that is new unless one is given (to stand for the same device after a restart).
    public static FrontendClient Create(ApiFactory factory, FakeSessionStorage? sessionStorage = null, string? folder = null)
    {
        bool ownsFolder = folder is null;
        folder ??= Path.Combine(Path.GetTempPath(), $"frontend-{Guid.CreateVersion7()}");
        Directory.CreateDirectory(folder);

        return new(factory, sessionStorage ?? new(), folder, ownsFolder);
    }

    // A frontend that signed in and started up: the library synced and the hub connected.
    public static async Task<FrontendClient> CreateSignedIn(ApiFactory factory, string email, string password = "a-long-enough-password")
    {
        FrontendClient frontend = Create(factory);
        await frontend.Session.SignIn(email, password);
        await frontend.Startup.Start();
        await WaitFor(() => Task.FromResult(frontend.Remote.DeviceList.Count > 0), "the device to be registered");

        return frontend;
    }

    // Polls until the condition holds, since pushes and the worker are asynchronous.
    public static async Task WaitFor(Func<Task<bool>> condition, string what, int seconds = 15)
    {
        DateTime deadline = DateTime.UtcNow.AddSeconds(seconds);
        while (!await condition())
        {
            Assert.True(DateTime.UtcNow < deadline, $"Timed out waiting for: {what}.");
            await Task.Delay(25, TestContext.Current.CancellationToken);
        }
    }

    public async ValueTask DisposeAsync()
    {
        await Hub.Stop();
        SqliteConnection.ClearAllPools();
        if (!_ownsFolder || !Directory.Exists(Folder)) return;

        try
        {
            Directory.Delete(Folder, true);
        }
        catch (IOException)
        {
            // A resync that was still finishing when the test ended can hold the file for a moment; a temp folder is not worth failing a test for.
        }
    }
}
