using Microsoft.AspNetCore.Http.Connections.Client;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Noxy.NET.MediaPlayer.Application;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;
using Noxy.NET.MediaPlayer.Domain.Responses.Setting;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;

namespace Noxy.NET.MediaPlayer.Presentation.Services;

public class MediaHubService(ISyncService serviceSync, ISettingService serviceSetting, IMetadataSyncService serviceMetadataSync, ISessionService serviceSession, IServerAddressStorage storageServerAddress, ILogger<MediaHubService> logger) : IMediaHubService
{
    public static IReadOnlyList<string> HandledEventNameList { get; } =
    [
        nameof(MediaHubEventEnum.UploadSucceeded),
        nameof(MediaHubEventEnum.MediaUpdated),
        nameof(MediaHubEventEnum.MediaDeleted),
        nameof(MediaHubEventEnum.PlaylistUpdated),
        nameof(MediaHubEventEnum.PlaylistDeleted),
        nameof(MediaHubEventEnum.SettingUpdated),
        nameof(MediaHubEventEnum.ArtistUpdated),
        nameof(MediaHubEventEnum.ArtistDeleted),
        nameof(MediaHubEventEnum.AlbumUpdated),
        nameof(MediaHubEventEnum.AlbumDeleted),
        nameof(MediaHubEventEnum.GenreUpdated),
        nameof(MediaHubEventEnum.GenreDeleted)
    ];

    private readonly SemaphoreSlim _lock = new(1, 1);
    private HubConnection? _connection;
    private bool _watchingSession;

    public bool IsConnected => _connection?.State == HubConnectionState.Connected;

    public async Task Start()
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

            if (_connection is not null) return;

            string? address = await storageServerAddress.GetAddress();
            if (address is null || !serviceSession.IsSignedIn) return;

            HubConnection connection = Build(address);
            try
            {
                await connection.StartAsync();
            }
            catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
            {
                logger.LogWarning(exception, "The media hub could not be reached.");
                await connection.DisposeAsync();
                return;
            }

            _connection = connection;
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
            .WithUrl(address + HubConstants.MediaHubPath, options =>
            {
                options.AccessTokenProvider = serviceSession.GetAccessToken;
                ConfigureConnection(options);
            })
            .WithAutomaticReconnect()
            .Build();

        connection.On<ResponseUploadResult>(nameof(MediaHubEventEnum.UploadSucceeded), x => Run(nameof(MediaHubEventEnum.UploadSucceeded), () => serviceSync.ApplyUploadSucceeded(x)));
        connection.On<ResponseMediaFile>(nameof(MediaHubEventEnum.MediaUpdated), x => Run(nameof(MediaHubEventEnum.MediaUpdated), () => serviceSync.ApplyMediaUpdated(x)));
        connection.On<Guid>(nameof(MediaHubEventEnum.MediaDeleted), x => Run(nameof(MediaHubEventEnum.MediaDeleted), () => serviceSync.ApplyMediaDeleted(x)));
        connection.On<ResponsePlaylist>(nameof(MediaHubEventEnum.PlaylistUpdated), x => Run(nameof(MediaHubEventEnum.PlaylistUpdated), () => serviceSync.ApplyPlaylistUpdated(x)));
        connection.On<Guid>(nameof(MediaHubEventEnum.PlaylistDeleted), x => Run(nameof(MediaHubEventEnum.PlaylistDeleted), () => serviceSync.ApplyPlaylistDeleted(x)));
        connection.On<ResponseSetting>(nameof(MediaHubEventEnum.SettingUpdated), x => Run(nameof(MediaHubEventEnum.SettingUpdated), () => serviceSetting.ApplySettingUpdated(x)));
        connection.On<ResponseArtist>(nameof(MediaHubEventEnum.ArtistUpdated), x => Run(nameof(MediaHubEventEnum.ArtistUpdated), () => serviceMetadataSync.ApplyArtistUpdated(x)));
        connection.On<Guid>(nameof(MediaHubEventEnum.ArtistDeleted), x => Run(nameof(MediaHubEventEnum.ArtistDeleted), () => serviceMetadataSync.ApplyArtistDeleted(x)));
        connection.On<ResponseAlbum>(nameof(MediaHubEventEnum.AlbumUpdated), x => Run(nameof(MediaHubEventEnum.AlbumUpdated), () => serviceMetadataSync.ApplyAlbumUpdated(x)));
        connection.On<Guid>(nameof(MediaHubEventEnum.AlbumDeleted), x => Run(nameof(MediaHubEventEnum.AlbumDeleted), () => serviceMetadataSync.ApplyAlbumDeleted(x)));
        connection.On<ResponseGenre>(nameof(MediaHubEventEnum.GenreUpdated), x => Run(nameof(MediaHubEventEnum.GenreUpdated), () => serviceMetadataSync.ApplyGenreUpdated(x)));
        connection.On<Guid>(nameof(MediaHubEventEnum.GenreDeleted), x => Run(nameof(MediaHubEventEnum.GenreDeleted), () => serviceMetadataSync.ApplyGenreDeleted(x)));
        connection.Reconnected += _ => Run(nameof(HubConnection.Reconnected), SyncAfterReconnect);

        // Automatic reconnect gave up: forget the connection so the next Start builds a new one.
        connection.Closed += _ =>
        {
            if (ReferenceEquals(_connection, connection)) _connection = null;
            return Task.CompletedTask;
        };

        return connection;
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
