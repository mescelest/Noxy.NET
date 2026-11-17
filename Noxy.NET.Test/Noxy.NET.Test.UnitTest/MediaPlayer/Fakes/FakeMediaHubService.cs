using Noxy.NET.MediaPlayer.Application.Requests.Remote;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Responses.Remote;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;

namespace Noxy.NET.Test.MediaPlayer.Fakes;

public sealed class FakeMediaHubService : IMediaHubService
{
    private readonly Lock _callLock = new();
    private bool? _isConnected;

    // Runs inside Start, so a test can record what had happened by then.
    public Action? OnStart { get; set; }

    public int StartCount { get; private set; }

    // Connected once started, unless a test says otherwise.
    public bool IsConnected { get => _isConnected ?? StartCount > 0; set => _isConnected = value; }

    // Every Invoke and Send, in order: the method and its argument.
    public List<(MediaHubMethodEnum Method, object? Argument)> CallList { get; } = [];

    // Answers each Invoke, or throws to script a failure. The default answers nothing, as an unconnected hub would.
    public Func<MediaHubMethodEnum, object?, object?> InvokeHandler { get; set; } = (_, _) => null;

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

    public Task Start()
    {
        StartCount++;
        OnStart?.Invoke();

        return Task.CompletedTask;
    }

    public Task Stop()
    {
        return Task.CompletedTask;
    }

    // When set, every Invoke and Send waits for it, so a test can hold a call in flight.
    public Task? Gate { get; set; }

    public int InFlightCount { get; private set; }

    public int MaxInFlightCount { get; private set; }

    public async Task<T?> Invoke<T>(MediaHubMethodEnum method, object? argument)
    {
        lock (_callLock) CallList.Add((method, argument));
        await Enter();
        try
        {
            return InvokeHandler(method, argument) is T answer ? answer : default;
        }
        finally
        {
            InFlightCount--;
        }
    }

    private async Task Enter()
    {
        InFlightCount++;
        MaxInFlightCount = Math.Max(MaxInFlightCount, InFlightCount);
        if (Gate is not null) await Gate;
    }

    public async Task Send(MediaHubMethodEnum method, object? argument)
    {
        lock (_callLock) CallList.Add((method, argument));
        await Enter();
        try
        {
            InvokeHandler(method, argument);
        }
        finally
        {
            InFlightCount--;
        }
    }

    public List<object?> ArgumentList(MediaHubMethodEnum method)
    {
        return CallList.Where(x => x.Method == method).Select(x => x.Argument).ToList();
    }

    public void RaiseConnected()
    {
        Connected?.Invoke();
    }

    public void RaiseDisconnected()
    {
        Disconnected?.Invoke();
    }

    public void RaiseDeviceListChanged(List<ResponseDevice> deviceList)
    {
        DeviceListChanged?.Invoke(deviceList);
    }

    public void RaisePlayerStateChanged(ResponsePlayerState state)
    {
        PlayerStateChanged?.Invoke(state);
    }

    public void RaiseQueueChanged(ResponseQueueDelta delta)
    {
        QueueChanged?.Invoke(delta);
    }

    public void RaiseQueueSnapshot(ResponseQueue queue)
    {
        QueueSnapshot?.Invoke(queue);
    }

    public void RaiseSessionEnded()
    {
        SessionEnded?.Invoke();
    }

    public void RaiseTransferFailed()
    {
        TransferFailed?.Invoke();
    }

    public void RaisePlayerCommandReceived(RequestPlayerCommand command)
    {
        PlayerCommandReceived?.Invoke(command);
    }

    public void RaisePlayerTransferRequested()
    {
        PlayerTransferRequested?.Invoke();
    }

    public void RaisePlayerStartRequested(ResponsePlayerState state, ResponseQueue queue)
    {
        PlayerStartRequested?.Invoke(state, queue);
    }
}
