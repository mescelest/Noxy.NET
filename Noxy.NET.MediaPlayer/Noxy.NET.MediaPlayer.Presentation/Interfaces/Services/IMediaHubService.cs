using Noxy.NET.MediaPlayer.Application.Requests.Remote;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Responses.Remote;

namespace Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;

// The live connection to the server's hub. Library, playlist, setting and metadata pushes are forwarded to their services; after a reconnect those lists are synced again, since pushes sent while offline are lost. The playback events are forwarded as they are.
public interface IMediaHubService
{
    bool IsConnected { get; }

    // Raised each time the connection is made: after Start, and after the connection came back by itself.
    event Action? Connected;

    // Raised when the connection is lost (it may come back by itself) or closed.
    event Action? Disconnected;

    event Action<List<ResponseDevice>>? DeviceListChanged;

    event Action<ResponsePlayerState>? PlayerStateChanged;

    event Action<ResponseQueueDelta>? QueueChanged;

    event Action<ResponseQueue>? QueueSnapshot;

    event Action? SessionEnded;

    event Action? TransferFailed;

    event Action<RequestPlayerCommand>? PlayerCommandReceived;

    event Action? PlayerTransferRequested;

    event Action<ResponsePlayerState, ResponseQueue>? PlayerStartRequested;

    // Does nothing without a saved server address or a signed-in user, and never throws when the server cannot be reached.
    Task Start();

    Task Stop();

    // Calls a hub method and returns its answer. Null, with no throw, when the connection is not up. A hub method that refuses the call throws HubException.
    Task<T?> Invoke<T>(MediaHubMethodEnum method, object? argument);

    // The same for a method with no answer: nothing happens when the connection is not up.
    Task Send(MediaHubMethodEnum method, object? argument);
}
