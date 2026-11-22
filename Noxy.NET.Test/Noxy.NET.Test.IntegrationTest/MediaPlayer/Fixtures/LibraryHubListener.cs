using System.Threading.Channels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.TestHost;
using Noxy.NET.MediaPlayer.API;
using Noxy.NET.MediaPlayer.Application;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;
using Noxy.NET.MediaPlayer.Domain.Responses.Setting;

namespace Noxy.NET.Test.MediaPlayer.Fixtures;

public sealed class LibraryHubListener : IAsyncDisposable
{
    private readonly HubConnection _connection;
    private readonly Channel<(string Event, ResponseUploadResult Result)> _messages = Channel.CreateUnbounded<(string, ResponseUploadResult)>();
    private readonly Channel<ResponseMediaFile> _updatedMessages = Channel.CreateUnbounded<ResponseMediaFile>();
    private readonly Channel<Guid> _deletedMessages = Channel.CreateUnbounded<Guid>();
    private readonly Channel<ResponsePlaylist> _playlistUpdatedMessages = Channel.CreateUnbounded<ResponsePlaylist>();
    private readonly Channel<Guid> _playlistDeletedMessages = Channel.CreateUnbounded<Guid>();
    private readonly Channel<ResponseArtist> _artistUpdatedMessages = Channel.CreateUnbounded<ResponseArtist>();
    private readonly Channel<Guid> _artistDeletedMessages = Channel.CreateUnbounded<Guid>();
    private readonly Channel<ResponseAlbum> _albumUpdatedMessages = Channel.CreateUnbounded<ResponseAlbum>();
    private readonly Channel<Guid> _albumDeletedMessages = Channel.CreateUnbounded<Guid>();
    private readonly Channel<ResponseGenre> _genreUpdatedMessages = Channel.CreateUnbounded<ResponseGenre>();
    private readonly Channel<Guid> _genreDeletedMessages = Channel.CreateUnbounded<Guid>();
    private readonly Channel<ResponseSetting> _settingUpdatedMessages = Channel.CreateUnbounded<ResponseSetting>();
    private readonly Channel<string> _anyMessages = Channel.CreateUnbounded<string>();

    private LibraryHubListener(HubConnection connection)
    {
        _connection = connection;
        connection.On<ResponseUploadResult>(nameof(MediaHubEventEnum.UploadSucceeded), result => Receive(nameof(MediaHubEventEnum.UploadSucceeded), () => _messages.Writer.TryWrite((nameof(MediaHubEventEnum.UploadSucceeded), result))));
        connection.On<ResponseUploadResult>(nameof(MediaHubEventEnum.UploadFailed), result => Receive(nameof(MediaHubEventEnum.UploadFailed), () => _messages.Writer.TryWrite((nameof(MediaHubEventEnum.UploadFailed), result))));
        connection.On<ResponseMediaFile>(nameof(MediaHubEventEnum.MediaUpdated), mediaFile => Receive(nameof(MediaHubEventEnum.MediaUpdated), () => _updatedMessages.Writer.TryWrite(mediaFile)));
        connection.On<Guid>(nameof(MediaHubEventEnum.MediaDeleted), id => Receive(nameof(MediaHubEventEnum.MediaDeleted), () => _deletedMessages.Writer.TryWrite(id)));
        connection.On<ResponsePlaylist>(nameof(MediaHubEventEnum.PlaylistUpdated), playlist => Receive(nameof(MediaHubEventEnum.PlaylistUpdated), () => _playlistUpdatedMessages.Writer.TryWrite(playlist)));
        connection.On<Guid>(nameof(MediaHubEventEnum.PlaylistDeleted), id => Receive(nameof(MediaHubEventEnum.PlaylistDeleted), () => _playlistDeletedMessages.Writer.TryWrite(id)));
        connection.On<ResponseArtist>(nameof(MediaHubEventEnum.ArtistUpdated), artist => Receive(nameof(MediaHubEventEnum.ArtistUpdated), () => _artistUpdatedMessages.Writer.TryWrite(artist)));
        connection.On<Guid>(nameof(MediaHubEventEnum.ArtistDeleted), id => Receive(nameof(MediaHubEventEnum.ArtistDeleted), () => _artistDeletedMessages.Writer.TryWrite(id)));
        connection.On<ResponseAlbum>(nameof(MediaHubEventEnum.AlbumUpdated), album => Receive(nameof(MediaHubEventEnum.AlbumUpdated), () => _albumUpdatedMessages.Writer.TryWrite(album)));
        connection.On<Guid>(nameof(MediaHubEventEnum.AlbumDeleted), id => Receive(nameof(MediaHubEventEnum.AlbumDeleted), () => _albumDeletedMessages.Writer.TryWrite(id)));
        connection.On<ResponseGenre>(nameof(MediaHubEventEnum.GenreUpdated), genre => Receive(nameof(MediaHubEventEnum.GenreUpdated), () => _genreUpdatedMessages.Writer.TryWrite(genre)));
        connection.On<Guid>(nameof(MediaHubEventEnum.GenreDeleted), id => Receive(nameof(MediaHubEventEnum.GenreDeleted), () => _genreDeletedMessages.Writer.TryWrite(id)));
        connection.On<ResponseSetting>(nameof(MediaHubEventEnum.SettingUpdated), setting => Receive(nameof(MediaHubEventEnum.SettingUpdated), () => _settingUpdatedMessages.Writer.TryWrite(setting)));
    }

    // Connects over WebSockets. A .NET client sends the token as a header; a browser client puts it in the query string.
    public static async Task<LibraryHubListener> Connect(ApiFactory factory, string? token, bool tokenInQueryString = false)
    {
        HubConnection connection = new HubConnectionBuilder()
            .WithUrl(new Uri(factory.Server.BaseAddress, ApplicationConstants.MediaHubPath), options =>
            {
                options.Transports = HttpTransportType.WebSockets;
                options.AccessTokenProvider = () => Task.FromResult(token);
                options.HttpMessageHandlerFactory = _ => factory.Server.CreateHandler();
                options.WebSocketFactory = async (context, cancellationToken) =>
                {
                    WebSocketClient client = factory.Server.CreateWebSocketClient();
                    Uri uri = context.Uri;

                    if (token is not null && tokenInQueryString)
                    {
                        uri = new UriBuilder(uri) { Query = $"{uri.Query.TrimStart('?')}&{ApiConstants.AccessTokenQueryKey}={Uri.EscapeDataString(token)}" }.Uri;
                    }
                    else if (token is not null)
                    {
                        client.ConfigureRequest = request => request.Headers.Authorization = $"{JwtBearerDefaults.AuthenticationScheme} {token}";
                    }

                    return await client.ConnectAsync(uri, cancellationToken);
                };
            })
            .Build();

        LibraryHubListener listener = new(connection);
        await connection.StartAsync(TestContext.Current.CancellationToken);
        return listener;
    }

    public async Task<(string Event, ResponseUploadResult Result)> Next()
    {
        return await Read(_messages);
    }

    public async Task<ResponseMediaFile> NextMediaUpdated()
    {
        return await Read(_updatedMessages);
    }

    public async Task<Guid> NextMediaDeleted()
    {
        return await Read(_deletedMessages);
    }

    public async Task<ResponsePlaylist> NextPlaylistUpdated()
    {
        return await Read(_playlistUpdatedMessages);
    }

    public async Task<ResponseArtist> NextArtistUpdated()
    {
        return await Read(_artistUpdatedMessages);
    }

    public async Task<Guid> NextArtistDeleted()
    {
        return await Read(_artistDeletedMessages);
    }

    public async Task<ResponseAlbum> NextAlbumUpdated()
    {
        return await Read(_albumUpdatedMessages);
    }

    public async Task<Guid> NextAlbumDeleted()
    {
        return await Read(_albumDeletedMessages);
    }

    public async Task<ResponseGenre> NextGenreUpdated()
    {
        return await Read(_genreUpdatedMessages);
    }

    public async Task<Guid> NextGenreDeleted()
    {
        return await Read(_genreDeletedMessages);
    }

    public async Task<ResponseSetting> NextSettingUpdated()
    {
        return await Read(_settingUpdatedMessages);
    }

    public async Task<Guid> NextPlaylistDeleted()
    {
        return await Read(_playlistDeletedMessages);
    }

    private void Receive(string name, Action store)
    {
        store();
        _anyMessages.Writer.TryWrite(name);
    }

    private static async Task<T> Read<T>(Channel<T> channel)
    {
        using CancellationTokenSource timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(15));

        return await channel.Reader.ReadAsync(timeout.Token);
    }

    // How many events this connection has received in total, after waiting a moment for stragglers.
    public async Task<int> CountReceivedAfter(TimeSpan wait)
    {
        await Task.Delay(wait, TestContext.Current.CancellationToken);

        return _anyMessages.Reader.Count;
    }

    public async Task<bool> ReceivesNothingWithin(TimeSpan time)
    {
        using CancellationTokenSource timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        timeout.CancelAfter(time);

        try
        {
            await _anyMessages.Reader.WaitToReadAsync(timeout.Token);
            return false;
        }
        catch (OperationCanceledException) when (!TestContext.Current.CancellationToken.IsCancellationRequested)
        {
            return true;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
    }
}
