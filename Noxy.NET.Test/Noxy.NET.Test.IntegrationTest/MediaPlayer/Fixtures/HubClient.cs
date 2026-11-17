using System.Collections.Concurrent;
using System.Threading.Channels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.TestHost;
using Noxy.NET.MediaPlayer.Application;
using Noxy.NET.MediaPlayer.Application.Requests.Remote;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Models;
using Noxy.NET.MediaPlayer.Domain.Responses.Remote;

namespace Noxy.NET.Test.MediaPlayer.Fixtures;

public sealed class HubClient : IAsyncDisposable
{
    private static readonly TimeSpan WaitTime = TimeSpan.FromSeconds(5);

    private readonly HubConnection _connection;
    private readonly ConcurrentDictionary<string, Channel<object?>> _channelMap = new();

    private HubClient(HubConnection connection, Guid deviceID, string name)
    {
        _connection = connection;
        DeviceID = deviceID;
        Name = name;

        connection.On<List<ResponseDevice>>(nameof(MediaHubEventEnum.DeviceListChanged), x => Receive(MediaHubEventEnum.DeviceListChanged, x));
        connection.On<ResponsePlayerState>(nameof(MediaHubEventEnum.PlayerStateChanged), x => Receive(MediaHubEventEnum.PlayerStateChanged, x));
        connection.On<ResponseQueueDelta>(nameof(MediaHubEventEnum.QueueChanged), x => Receive(MediaHubEventEnum.QueueChanged, x));
        connection.On<ResponseQueue>(nameof(MediaHubEventEnum.QueueSnapshot), x => Receive(MediaHubEventEnum.QueueSnapshot, x));
        connection.On(nameof(MediaHubEventEnum.SessionEnded), () => Receive(MediaHubEventEnum.SessionEnded, null));
        connection.On(nameof(MediaHubEventEnum.TransferFailed), () => Receive(MediaHubEventEnum.TransferFailed, null));
        connection.On<RequestPlayerCommand>(nameof(MediaHubEventEnum.PlayerCommandReceived), x => Receive(MediaHubEventEnum.PlayerCommandReceived, x));
        connection.On(nameof(MediaHubEventEnum.PlayerTransferRequested), () => Receive(MediaHubEventEnum.PlayerTransferRequested, null));
        connection.On<ResponsePlayerState, ResponseQueue>(nameof(MediaHubEventEnum.PlayerStartRequested), (state, queue) => Receive(MediaHubEventEnum.PlayerStartRequested, (state, queue)));
    }

    public Guid DeviceID { get; }
    public string Name { get; }

    // Connects over WebSockets as the given account, and registers the device unless told not to.
    public static async Task<HubClient> Connect(ApiFactory factory, string? token, string name = "Device", Guid? deviceID = null, bool register = true)
    {
        HubConnection connection = new HubConnectionBuilder()
            .WithUrl(new Uri(factory.Server.BaseAddress, HubConstants.MediaHubPath), options =>
            {
                options.Transports = HttpTransportType.WebSockets;
                options.AccessTokenProvider = () => Task.FromResult(token);
                options.HttpMessageHandlerFactory = _ => factory.Server.CreateHandler();
                options.WebSocketFactory = async (context, cancellationToken) =>
                {
                    WebSocketClient client = factory.Server.CreateWebSocketClient();
                    if (token is not null) client.ConfigureRequest = request => request.Headers.Authorization = $"{JwtBearerDefaults.AuthenticationScheme} {token}";

                    return await client.ConnectAsync(context.Uri, cancellationToken);
                };
            })
            .Build();

        HubClient client = new(connection, deviceID ?? Guid.CreateVersion7(), name);
        await connection.StartAsync(TestContext.Current.CancellationToken);
        if (register) await client.Register();

        return client;
    }

    public async Task<List<ResponseDevice>> Register()
    {
        return await _connection.InvokeAsync<List<ResponseDevice>>(nameof(MediaHubMethodEnum.RegisterDevice), new RequestRegisterDevice { DeviceID = DeviceID, Name = Name }, TestContext.Current.CancellationToken);
    }

    public async Task<bool> Claim(RequestClaimActive request)
    {
        return await _connection.InvokeAsync<bool>(nameof(MediaHubMethodEnum.ClaimActive), request, TestContext.Current.CancellationToken);
    }

    public async Task Report(RequestReportPlayerState state)
    {
        await _connection.InvokeAsync(nameof(MediaHubMethodEnum.ReportState), state, TestContext.Current.CancellationToken);
    }

    public async Task Edit(QueueEdit edit)
    {
        await _connection.InvokeAsync(nameof(MediaHubMethodEnum.EditQueue), edit, TestContext.Current.CancellationToken);
    }

    public async Task<ResponseQueue?> RequestSnapshot()
    {
        return await _connection.InvokeAsync<ResponseQueue?>(nameof(MediaHubMethodEnum.RequestSnapshot), TestContext.Current.CancellationToken);
    }

    public async Task<CommandResultEnum> SendCommand(RequestPlayerCommand command)
    {
        return await _connection.InvokeAsync<CommandResultEnum>(nameof(MediaHubMethodEnum.SendCommand), command, TestContext.Current.CancellationToken);
    }

    public async Task<bool> Transfer(Guid targetDeviceID)
    {
        return await _connection.InvokeAsync<bool>(nameof(MediaHubMethodEnum.Transfer), targetDeviceID, TestContext.Current.CancellationToken);
    }

    // The next event of the kind, in the order they arrived; fails the test when none comes in time.
    public async Task<T> Next<T>(MediaHubEventEnum eventKind)
    {
        using CancellationTokenSource timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        timeout.CancelAfter(WaitTime);

        object? payload = await ChannelOf(eventKind).Reader.ReadAsync(timeout.Token);
        return payload is T typed ? typed : throw new InvalidOperationException($"The {eventKind} event did not carry a {typeof(T).Name}.");
    }

    // For events that carry nothing.
    public async Task Next(MediaHubEventEnum eventKind)
    {
        using CancellationTokenSource timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        timeout.CancelAfter(WaitTime);

        await ChannelOf(eventKind).Reader.ReadAsync(timeout.Token);
    }

    // True when no event of the kind arrives within the time.
    public async Task<bool> ReceivesNone(MediaHubEventEnum eventKind, TimeSpan time)
    {
        using CancellationTokenSource timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        timeout.CancelAfter(time);

        try
        {
            await ChannelOf(eventKind).Reader.WaitToReadAsync(timeout.Token);
            return false;
        }
        catch (OperationCanceledException) when (!TestContext.Current.CancellationToken.IsCancellationRequested)
        {
            return true;
        }
    }

    // True when no event at all arrives within the time.
    public async Task<bool> ReceivesNothing(TimeSpan time)
    {
        await Task.Delay(time, TestContext.Current.CancellationToken);

        return _channelMap.Values.All(x => x.Reader.Count == 0);
    }

    // Forgets the events of what just happened, so a test sees only what comes after. The server sends to one connection in order, so once a call of this client has returned, everything sent to it before has arrived.
    public async Task Settle()
    {
        await RequestSnapshot();
        Drain();
    }

    public void Drain()
    {
        foreach (Channel<object?> channel in _channelMap.Values)
        {
            while (channel.Reader.TryRead(out _)) { }
        }
    }

    public async Task Stop()
    {
        await _connection.StopAsync(TestContext.Current.CancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
    }

    private Channel<object?> ChannelOf(MediaHubEventEnum eventKind)
    {
        return _channelMap.GetOrAdd(eventKind.ToString(), _ => Channel.CreateUnbounded<object?>());
    }

    private void Receive(MediaHubEventEnum eventKind, object? payload)
    {
        ChannelOf(eventKind).Writer.TryWrite(payload);
    }
}
