using Microsoft.AspNetCore.Http.Connections.Client;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Noxy.NET.MediaPlayer.Application;
using Noxy.NET.MediaPlayer.Application.Requests.Remote;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;
using Noxy.NET.MediaPlayer.Domain.Responses.Remote;
using Noxy.NET.MediaPlayer.Domain.Responses.Setting;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;

namespace Noxy.NET.MediaPlayer.Presentation.Services;

public class MediaHubService(ISyncService serviceSync, ISettingService serviceSetting, IMetadataSyncService serviceMetadataSync, ISessionService serviceSession, IServerAddressStorage storageServerAddress, ILogger<MediaHubService> logger) : IMediaHubService
{
    public static IReadOnlyList<string> HandledEventNameList { get; } =
    [
        nameof(MediaHubEventEnum.UploadSucceeded),
        nameof(MediaHubEventEnum.UploadFailed),
        nameof(MediaHubEventEnum.MediaUpdated),
        nameof(MediaHubEventEnum.MediaDeleted),
        nameof(MediaHubEventEnum.PlaylistUpdated),
        nameof(MediaHubEventEnum.PlaylistDeleted),
        nameof(MediaHubEventEnum.PlaylistOrderChanged),
        nameof(MediaHubEventEnum.SettingUpdated),
        nameof(MediaHubEventEnum.ArtistUpdated),
        nameof(MediaHubEventEnum.ArtistDeleted),
        nameof(MediaHubEventEnum.AlbumUpdated),
        nameof(MediaHubEventEnum.AlbumDeleted),
        nameof(MediaHubEventEnum.GenreUpdated),
        nameof(MediaHubEventEnum.GenreDeleted),
        nameof(MediaHubEventEnum.DeviceListChanged),
        nameof(MediaHubEventEnum.PlayerStateChanged),
        nameof(MediaHubEventEnum.QueueChanged),
        nameof(MediaHubEventEnum.QueueSnapshot),
        nameof(MediaHubEventEnum.SessionEnded),
        nameof(MediaHubEventEnum.TransferFailed),
        nameof(MediaHubEventEnum.PlayerCommandReceived),
        nameof(MediaHubEventEnum.PlayerTransferRequested),
        nameof(MediaHubEventEnum.PlayerStartRequested)
    ];

    private readonly SemaphoreSlim _lock = new(1, 1);
    private HubConnection? _connection;
    private bool _watchingSession;

    public bool IsConnected => _connection?.State == HubConnectionState.Connected;

    public event Action? Connected;

    public event Action? Disconnected;

    public event Action<List<ResponseDevice>>? DeviceListChanged;

    public event Action<ResponsePlayerState>? PlayerStateChanged;

    public event Action<ResponseQueueDelta>? QueueChanged;

    public event Action<ResponseQueue>? QueueSnapshot;

    public event Action? SessionEnded;

    public event Action? TransferFailed;

    public event Action<RequestPlayerCommand>? PlayerCommandReceived;

    public event Action? PlayerTransferRequested;

    public event Action<ResponsePlayerState, ResponseQueue>? PlayerStartRequested;

    public async Task Start()
    {
        bool started = await StartConnection();
        if (started) Connected?.Invoke();
    }

    // True when this call made the connection.
    private async Task<bool> StartConnection()
    {
        await _lock.WaitAsync();
        try
        {
            // A signed-out user must stop receiving that account's pushes, whichever way the session ended (sign-out, or a refresh that was rejected).
            if (!_watchingSession)
            {
                serviceSession.Changed += OnSessionChanged;
                _watchingSession = true;
            }

            if (_connection is not null) return false;

            string? address = await storageServerAddress.GetAddress();
            if (address is null || !serviceSession.IsSignedIn) return false;

            HubConnection connection = Build(address);
            try
            {
                await connection.StartAsync();
            }
            catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
            {
                logger.LogWarning(exception, "The media hub could not be reached.");
                await connection.DisposeAsync();
                return false;
            }

            _connection = connection;
            return true;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task Stop()
    {
        await _lock.WaitAsync();
        try
        {
            if (_connection is null) return;

            HubConnection connection = _connection;
            _connection = null;
            await connection.DisposeAsync();
        }
        finally
        {
            _lock.Release();
        }
    }

    private void OnSessionChanged()
    {
        if (!serviceSession.IsSignedIn) _ = Run(nameof(ISessionService.Changed), Stop);
    }

    // What the connection missed while it was down. Each sync has its own try, so one that fails does not skip the others.
    public async Task SyncAfterReconnect()
    {
        await Run(nameof(ISyncService.SyncLibrary), () => serviceSync.SyncLibrary());
        await Run(nameof(ISyncService.SyncPlaylistList), () => serviceSync.SyncPlaylistList());
        await Run(nameof(ISettingService.SyncSettingList), () => serviceSetting.SyncSettingList());
        await Run(nameof(IMetadataSyncService.SyncArtistList), () => serviceMetadataSync.SyncArtistList());
        await Run(nameof(IMetadataSyncService.SyncAlbumList), () => serviceMetadataSync.SyncAlbumList());
        await Run(nameof(IMetadataSyncService.SyncGenreList), () => serviceMetadataSync.SyncGenreList());
    }

    // A place for tests to swap the transport (for example to reach an in-memory test server). The app leaves the defaults.
    protected virtual void ConfigureConnection(HttpConnectionOptions options) { }

    private HubConnection Build(string address)
    {
        HubConnection connection = new HubConnectionBuilder()
            .WithUrl(address + ApplicationConstants.MediaHubPath, options =>
            {
                options.AccessTokenProvider = serviceSession.GetAccessToken;
                ConfigureConnection(options);
            })
            .WithAutomaticReconnect()
            .Build();

        connection.On<ResponseUploadResult>(nameof(MediaHubEventEnum.UploadSucceeded), x => Run(nameof(MediaHubEventEnum.UploadSucceeded), () => serviceSync.ApplyUploadSucceeded(x)));
        connection.On<ResponseUploadResult>(nameof(MediaHubEventEnum.UploadFailed), x => Run(nameof(MediaHubEventEnum.UploadFailed), () => serviceSync.ApplyUploadFailed(x)));
        connection.On<ResponseMediaFile>(nameof(MediaHubEventEnum.MediaUpdated), x => Run(nameof(MediaHubEventEnum.MediaUpdated), () => serviceSync.ApplyMediaUpdated(x)));
        connection.On<Guid>(nameof(MediaHubEventEnum.MediaDeleted), x => Run(nameof(MediaHubEventEnum.MediaDeleted), () => serviceSync.ApplyMediaDeleted(x)));
        connection.On<ResponsePlaylist>(nameof(MediaHubEventEnum.PlaylistUpdated), x => Run(nameof(MediaHubEventEnum.PlaylistUpdated), () => serviceSync.ApplyPlaylistUpdated(x)));
        connection.On<Guid>(nameof(MediaHubEventEnum.PlaylistDeleted), x => Run(nameof(MediaHubEventEnum.PlaylistDeleted), () => serviceSync.ApplyPlaylistDeleted(x)));
        // Positions moved, which a single playlist does not say: the list is read again, past the ETag.
        connection.On(nameof(MediaHubEventEnum.PlaylistOrderChanged), () => Run(nameof(MediaHubEventEnum.PlaylistOrderChanged), () => serviceSync.SyncPlaylistList(true)));
        connection.On<ResponseSetting>(nameof(MediaHubEventEnum.SettingUpdated), x => Run(nameof(MediaHubEventEnum.SettingUpdated), () => serviceSetting.ApplySettingUpdated(x)));
        connection.On<ResponseArtist>(nameof(MediaHubEventEnum.ArtistUpdated), x => Run(nameof(MediaHubEventEnum.ArtistUpdated), () => serviceMetadataSync.ApplyArtistUpdated(x)));
        connection.On<Guid>(nameof(MediaHubEventEnum.ArtistDeleted), x => Run(nameof(MediaHubEventEnum.ArtistDeleted), () => serviceMetadataSync.ApplyArtistDeleted(x)));
        connection.On<ResponseAlbum>(nameof(MediaHubEventEnum.AlbumUpdated), x => Run(nameof(MediaHubEventEnum.AlbumUpdated), () => serviceMetadataSync.ApplyAlbumUpdated(x)));
        connection.On<Guid>(nameof(MediaHubEventEnum.AlbumDeleted), x => Run(nameof(MediaHubEventEnum.AlbumDeleted), () => serviceMetadataSync.ApplyAlbumDeleted(x)));
        connection.On<ResponseGenre>(nameof(MediaHubEventEnum.GenreUpdated), x => Run(nameof(MediaHubEventEnum.GenreUpdated), () => serviceMetadataSync.ApplyGenreUpdated(x)));
        connection.On<Guid>(nameof(MediaHubEventEnum.GenreDeleted), x => Run(nameof(MediaHubEventEnum.GenreDeleted), () => serviceMetadataSync.ApplyGenreDeleted(x)));
        connection.On<List<ResponseDevice>>(nameof(MediaHubEventEnum.DeviceListChanged), x => Raise(nameof(MediaHubEventEnum.DeviceListChanged), () => DeviceListChanged?.Invoke(x)));
        connection.On<ResponsePlayerState>(nameof(MediaHubEventEnum.PlayerStateChanged), x => Raise(nameof(MediaHubEventEnum.PlayerStateChanged), () => PlayerStateChanged?.Invoke(x)));
        connection.On<ResponseQueueDelta>(nameof(MediaHubEventEnum.QueueChanged), x => Raise(nameof(MediaHubEventEnum.QueueChanged), () => QueueChanged?.Invoke(x)));
        connection.On<ResponseQueue>(nameof(MediaHubEventEnum.QueueSnapshot), x => Raise(nameof(MediaHubEventEnum.QueueSnapshot), () => QueueSnapshot?.Invoke(x)));
        connection.On(nameof(MediaHubEventEnum.SessionEnded), () => Raise(nameof(MediaHubEventEnum.SessionEnded), () => SessionEnded?.Invoke()));
        connection.On(nameof(MediaHubEventEnum.TransferFailed), () => Raise(nameof(MediaHubEventEnum.TransferFailed), () => TransferFailed?.Invoke()));
        connection.On<RequestPlayerCommand>(nameof(MediaHubEventEnum.PlayerCommandReceived), x => Raise(nameof(MediaHubEventEnum.PlayerCommandReceived), () => PlayerCommandReceived?.Invoke(x)));
        connection.On(nameof(MediaHubEventEnum.PlayerTransferRequested), () => Raise(nameof(MediaHubEventEnum.PlayerTransferRequested), () => PlayerTransferRequested?.Invoke()));
        connection.On<ResponsePlayerState, ResponseQueue>(nameof(MediaHubEventEnum.PlayerStartRequested), (state, queue) => Raise(nameof(MediaHubEventEnum.PlayerStartRequested), () => PlayerStartRequested?.Invoke(state, queue)));

        connection.Reconnecting += _ => Raise(nameof(HubConnection.Reconnecting), () => Disconnected?.Invoke());
        connection.Reconnected += _ => Run(nameof(HubConnection.Reconnected), async () =>
        {
            await SyncAfterReconnect();
            Connected?.Invoke();
        });

        // Automatic reconnect gave up: forget the connection so the next Start builds a new one.
        connection.Closed += _ =>
        {
            if (ReferenceEquals(_connection, connection)) _connection = null;
            return Raise(nameof(HubConnection.Closed), () => Disconnected?.Invoke());
        };

        return connection;
    }

    public async Task<T?> Invoke<T>(MediaHubMethodEnum method, object? argument)
    {
        HubConnection? connection = _connection;
        if (connection is null || connection.State != HubConnectionState.Connected) return default;

        try
        {
            return argument is null ? await connection.InvokeAsync<T>(method.ToString()) : await connection.InvokeAsync<T>(method.ToString(), argument);
        }
        catch (InvalidOperationException)
        {
            // The connection went down between the check and the call.
            return default;
        }
    }

    public async Task Send(MediaHubMethodEnum method, object? argument)
    {
        HubConnection? connection = _connection;
        if (connection is null || connection.State != HubConnectionState.Connected) return;

        try
        {
            if (argument is null) await connection.InvokeAsync(method.ToString());
            else await connection.InvokeAsync(method.ToString(), argument);
        }
        catch (InvalidOperationException)
        {
            // The connection went down between the check and the call.
        }
    }

    // Raises an event to the app's own handlers: one of them failing must not take the connection down either.
    private Task Raise(string eventName, Action raise)
    {
        return Run(eventName, () =>
        {
            raise();
            return Task.CompletedTask;
        });
    }

    // A failing handler must never take the connection down: the next sync repairs whatever it missed.
    private async Task Run(string eventName, Func<Task> handler)
    {
        try
        {
            await handler();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Handling the media hub event {EventName} failed.", eventName);
        }
    }
}
