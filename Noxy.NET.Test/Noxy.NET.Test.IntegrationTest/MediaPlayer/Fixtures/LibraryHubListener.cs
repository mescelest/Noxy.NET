using System.Threading.Channels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.TestHost;
using Noxy.NET.MediaPlayer.API;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;

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
    private readonly Channel<ResponseSettings> _settingsUpdatedMessages = Channel.CreateUnbounded<ResponseSettings>();
    private readonly Channel<string> _anyMessages = Channel.CreateUnbounded<string>();

    private LibraryHubListener(HubConnection connection)
    {
        _connection = connection;
        connection.On<ResponseUploadResult>(nameof(MediaHubEvent.UploadSucceeded), result => Receive(nameof(MediaHubEvent.UploadSucceeded), () => _messages.Writer.TryWrite((nameof(MediaHubEvent.UploadSucceeded), result))));
        connection.On<ResponseUploadResult>(nameof(MediaHubEvent.UploadFailed), result => Receive(nameof(MediaHubEvent.UploadFailed), () => _messages.Writer.TryWrite((nameof(MediaHubEvent.UploadFailed), result))));
        connection.On<ResponseMediaFile>(nameof(MediaHubEvent.MediaUpdated), mediaFile => Receive(nameof(MediaHubEvent.MediaUpdated), () => _updatedMessages.Writer.TryWrite(mediaFile)));
        connection.On<Guid>(nameof(MediaHubEvent.MediaDeleted), id => Receive(nameof(MediaHubEvent.MediaDeleted), () => _deletedMessages.Writer.TryWrite(id)));
        connection.On<ResponsePlaylist>(nameof(MediaHubEvent.PlaylistUpdated), playlist => Receive(nameof(MediaHubEvent.PlaylistUpdated), () => _playlistUpdatedMessages.Writer.TryWrite(playlist)));
        connection.On<Guid>(nameof(MediaHubEvent.PlaylistDeleted), id => Receive(nameof(MediaHubEvent.PlaylistDeleted), () => _playlistDeletedMessages.Writer.TryWrite(id)));
        connection.On<ResponseArtist>(nameof(MediaHubEvent.ArtistUpdated), artist => Receive(nameof(MediaHubEvent.ArtistUpdated), () => _artistUpdatedMessages.Writer.TryWrite(artist)));
        connection.On<Guid>(nameof(MediaHubEvent.ArtistDeleted), id => Receive(nameof(MediaHubEvent.ArtistDeleted), () => _artistDeletedMessages.Writer.TryWrite(id)));
        connection.On<ResponseAlbum>(nameof(MediaHubEvent.AlbumUpdated), album => Receive(nameof(MediaHubEvent.AlbumUpdated), () => _albumUpdatedMessages.Writer.TryWrite(album)));
        connection.On<Guid>(nameof(MediaHubEvent.AlbumDeleted), id => Receive(nameof(MediaHubEvent.AlbumDeleted), () => _albumDeletedMessages.Writer.TryWrite(id)));
        connection.On<ResponseGenre>(nameof(MediaHubEvent.GenreUpdated), genre => Receive(nameof(MediaHubEvent.GenreUpdated), () => _genreUpdatedMessages.Writer.TryWrite(genre)));
        connection.On<Guid>(nameof(MediaHubEvent.GenreDeleted), id => Receive(nameof(MediaHubEvent.GenreDeleted), () => _genreDeletedMessages.Writer.TryWrite(id)));
        connection.On<ResponseSettings>(nameof(MediaHubEvent.SettingsUpdated), settings => Receive(nameof(MediaHubEvent.SettingsUpdated), () => _settingsUpdatedMessages.Writer.TryWrite(settings)));
    }

    // Connects over WebSockets. A .NET client sends the token as a header; a browser client puts it in the query string.
    public static async Task<LibraryHubListener> Connect(ApiFactory factory, string? token, bool tokenInQueryString = false)
    {
        HubConnection connection = new HubConnectionBuilder()
            .WithUrl(new Uri(factory.Server.BaseAddress, Constants.MediaHubPath), options =>
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
                        uri = new UriBuilder(uri) { Query = $"{uri.Query.TrimStart('?')}&{Constants.AccessTokenQueryKey}={Uri.EscapeDataString(token)}" }.Uri;
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

    public async Task<ResponseSettings> NextSettingsUpdated()
    {
        return await Read(_settingsUpdatedMessages);
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
