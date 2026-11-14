using System.Threading.Channels;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.TestHost;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;

namespace Noxy.NET.Test.MediaPlayer.Api;

public sealed class UploadHubListener : IAsyncDisposable
{
    private readonly HubConnection _connection;
    private readonly Channel<(string Event, ResponseUploadResult Result)> _messages = Channel.CreateUnbounded<(string, ResponseUploadResult)>();

    private UploadHubListener(HubConnection connection)
    {
        _connection = connection;
        connection.On<ResponseUploadResult>("UploadSucceeded", result => _messages.Writer.TryWrite(("UploadSucceeded", result)));
        connection.On<ResponseUploadResult>("UploadFailed", result => _messages.Writer.TryWrite(("UploadFailed", result)));
    }

    // Connects over WebSockets. A .NET client sends the token as a header; a browser client puts it in the query string.
    public static async Task<UploadHubListener> Connect(ApiFactory factory, string? token, bool tokenInQueryString = false)
    {
        HubConnection connection = new HubConnectionBuilder()
            .WithUrl(new Uri(factory.Server.BaseAddress, "/hubs/media"), options =>
            {
                options.Transports = HttpTransportType.WebSockets;
                options.AccessTokenProvider = () => Task.FromResult(token);
                options.HttpMessageHandlerFactory = _ => factory.Server.CreateHandler();
                options.WebSocketFactory = async (context, cancellationToken) =>
                {
                    WebSocketClient client = factory.Server.CreateWebSocketClient();
                    Uri uri = context.Uri;

                    if (token != null && tokenInQueryString)
                    {
                        uri = new UriBuilder(uri) { Query = $"{uri.Query.TrimStart('?')}&access_token={Uri.EscapeDataString(token)}" }.Uri;
                    }
                    else if (token != null)
                    {
                        client.ConfigureRequest = request => request.Headers.Authorization = $"Bearer {token}";
                    }

                    return await client.ConnectAsync(uri, cancellationToken);
                };
            })
            .Build();

        UploadHubListener listener = new(connection);
        await connection.StartAsync(TestContext.Current.CancellationToken);
        return listener;
    }

    public async Task<(string Event, ResponseUploadResult Result)> Next()
    {
        using CancellationTokenSource timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(15));

        return await _messages.Reader.ReadAsync(timeout.Token);
    }

    public async Task<bool> ReceivesNothingWithin(TimeSpan time)
    {
        using CancellationTokenSource timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        timeout.CancelAfter(time);

        try
        {
            await _messages.Reader.WaitToReadAsync(timeout.Token);
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
