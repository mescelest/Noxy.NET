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

        Store = new SqliteMetadataStore(Path.Combine(folder, Constants.MetadataDatabaseFileName));
        Session = new(new AuthApiClient(httpClient, storageServerAddress), new FakeGoogleSignInService(), sessionStorage, Time);
        Api = new(httpClient, Session, storageServerAddress);
        Sync = new(Api, Store);
        SettingService = new(Api, Store);
        MetadataSync = new(Api, Store);
        Hub = new(Sync, SettingService, MetadataSync, Session, storageServerAddress, NullLogger<MediaHubService>.Instance, Network);

        FileStore = new(folder);
        Download = new(Api, FileStore, new HashService(), Store, new Delay(), NullLogger<DownloadService>.Instance);
        Reconciler = new(FileStore, Store, Download);
        Startup = new(Session, Sync, SettingService, MetadataSync, Hub, Reconciler, NullLogger<AppStartupService>.Instance);
        Player = new(Audio, Download, FileStore, Store, Sync, SettingService, NullLogger<PlayerService>.Instance);
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

    public PlayerService Player { get; }

    // A folder that is new unless one is given (to stand for the same device after a restart).
    public static FrontendClient Create(ApiFactory factory, FakeSessionStorage? sessionStorage = null, string? folder = null)
    {
        bool ownsFolder = folder is null;
        folder ??= Path.Combine(Path.GetTempPath(), $"frontend-{Guid.CreateVersion7()}");
        Directory.CreateDirectory(folder);

        return new(factory, sessionStorage ?? new(), folder, ownsFolder);
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
