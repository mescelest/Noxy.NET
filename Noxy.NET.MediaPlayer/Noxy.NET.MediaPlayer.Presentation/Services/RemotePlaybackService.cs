using Microsoft.Extensions.Logging;
using Noxy.NET.MediaPlayer.Application.Requests.Remote;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Responses.Remote;
using Noxy.NET.MediaPlayer.Presentation.Enums;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;
using Noxy.NET.MediaPlayer.Presentation.Models;

namespace Noxy.NET.MediaPlayer.Presentation.Services;

public class RemotePlaybackService : IRemotePlaybackService
{
    private readonly Lock _lock = new();
    private readonly IMediaHubService _serviceMediaHub;
    private readonly IDeviceService _serviceDevice;
    private readonly IQueueService _serviceQueue;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<RemotePlaybackService> _logger;

    // The queue's work (deltas, snapshots, registering) runs one step at a time in the order it arrived: a delta applied before the one ahead of it would look like a gap.
    private Task _tail = Task.CompletedTask;
    private List<ResponseDevice> _deviceList = [];
    private RemoteState? _remote;
    private Guid? _activeDeviceID;
    private Guid? _deviceID;
    private bool _hasSession;

    // While registering, the server pushes the queue and the state of a session that is already live before it answers. The queue is held back until the answer says whether somebody is active.
    private bool _registering;
    private ResponseQueue? _pendingSnapshot;

    // The highest position shown for the current report window, so that a late report does not make the position jump back.
    private double _floorSeconds;

    public RemotePlaybackService(IMediaHubService serviceMediaHub, IDeviceService serviceDevice, IQueueService serviceQueue, TimeProvider timeProvider, ILogger<RemotePlaybackService> logger)
    {
        _serviceMediaHub = serviceMediaHub;
        _serviceDevice = serviceDevice;
        _serviceQueue = serviceQueue;
        _timeProvider = timeProvider;
        _logger = logger;

        serviceMediaHub.Connected += () => Enqueue(Register);
        serviceMediaHub.Disconnected += OnDisconnected;
        serviceMediaHub.DeviceListChanged += OnDeviceListChanged;
        serviceMediaHub.PlayerStateChanged += OnPlayerStateChanged;
        serviceMediaHub.QueueChanged += delta => Enqueue(() => serviceQueue.ApplyDelta(delta));
        serviceMediaHub.QueueSnapshot += OnQueueSnapshot;
        serviceMediaHub.SessionEnded += OnSessionEnded;
        serviceMediaHub.TransferFailed += () => TransferFailed?.Invoke();
        serviceQueue.GapDetected += () => Enqueue(RequestSnapshot);
    }

    public event Action? Changed;

    public event Action? Registered;

    public event Action? TransferFailed;

    public PlaybackRoleEnum Role
    {
        get
        {
            lock (_lock)
            {
                if (!IsOnline || _activeDeviceID is null) return PlaybackRoleEnum.Local;

                return _activeDeviceID == _deviceID ? PlaybackRoleEnum.Active : PlaybackRoleEnum.Controller;
            }
        }
    }

    public List<ResponseDevice> DeviceList
    {
        get
        {
            lock (_lock) return [.. _deviceList];
        }
    }

    public RemoteState? Remote
    {
        get
        {
            lock (_lock) return _remote;
        }
    }

    public Guid? ActiveDeviceID
    {
        get
        {
            lock (_lock) return _activeDeviceID;
        }
    }

    public bool IsOnline => _serviceMediaHub.IsConnected;

    public bool HasSession
    {
        get
        {
            lock (_lock) return _hasSession;
        }
    }

    public TimeSpan GetPosition()
    {
        lock (_lock)
        {
            if (_remote is null) return TimeSpan.Zero;

            double seconds = _remote.State.PositionSeconds;
            if (_remote.State.IsPlaying) seconds += Math.Max(0, (Now() - _remote.ReceivedAt).TotalSeconds);

            _floorSeconds = Math.Max(seconds, _floorSeconds);

            return TimeSpan.FromSeconds(_floorSeconds);
        }
    }

    public async Task Register()
    {
        Guid deviceID = await _serviceDevice.GetDeviceID();
        string name = await _serviceDevice.GetName();
        lock (_lock)
        {
            _deviceID = deviceID;
            _registering = true;
            _pendingSnapshot = null;
        }

        List<ResponseDevice>? deviceList = null;
        ResponseQueue? pending;
        try
        {
            deviceList = await _serviceMediaHub.Invoke<List<ResponseDevice>>(MediaHubMethodEnum.RegisterDevice, new RequestRegisterDevice { DeviceID = deviceID, Name = name });
        }
        finally
        {
            lock (_lock)
            {
                _registering = false;
                pending = _pendingSnapshot;
                _pendingSnapshot = null;
                if (deviceList is not null) SetDeviceList(deviceList);
            }
        }

        // The queue of a session that somebody plays: this device is a controller (or the active one) and takes it. With nobody active the device keeps its own copy, which is what "Play here" resumes.
        if (pending is not null && ActiveDeviceID is not null) await ApplySnapshot(pending);

        Changed?.Invoke();
        if (deviceList is not null) Registered?.Invoke();
    }

    public void MarkActive()
    {
        lock (_lock)
        {
            _activeDeviceID = _deviceID;
            _hasSession = true;
        }

        Changed?.Invoke();
    }

    private DateTime Now()
    {
        return _timeProvider.GetUtcNow().UtcDateTime;
    }

    // Under _lock.
    private void SetDeviceList(List<ResponseDevice> deviceList)
    {
        _deviceList = [.. deviceList];
        _activeDeviceID = deviceList.FirstOrDefault(x => x.IsActive)?.DeviceID;
        if (_activeDeviceID is not null) _hasSession = true;
    }

    private void OnDeviceListChanged(List<ResponseDevice> deviceList)
    {
        lock (_lock) SetDeviceList(deviceList);

        Changed?.Invoke();
    }

    // A report older than the one held for the same active device is a late message. A new active device starts again at any number.
    private void OnPlayerStateChanged(ResponsePlayerState state)
    {
        lock (_lock)
        {
            if (_remote is { } held && held.State.ActiveDeviceID == state.ActiveDeviceID && state.StateRevision < held.State.StateRevision) return;

            // A report that is behind what was shown by a moment is a delay; one that is further behind (or of another item) is a seek.
            bool sameItem = _remote?.State.CurrentItemID == state.CurrentItemID;
            double behind = _floorSeconds - state.PositionSeconds;
            if (!(sameItem && behind is > 0 and <= Constants.RemotePositionToleranceSeconds)) _floorSeconds = state.PositionSeconds;

            _remote = new(state, Now());
            _activeDeviceID = state.ActiveDeviceID;
            _hasSession = true;
        }

        Changed?.Invoke();
    }

    // The session is over: the state goes, the local copy of the queue stays.
    private void OnSessionEnded()
    {
        lock (_lock)
        {
            _remote = null;
            _activeDeviceID = null;
            _hasSession = false;
            _floorSeconds = 0;
        }

        Changed?.Invoke();
    }

    private void OnDisconnected()
    {
        lock (_lock)
        {
            _remote = null;
            _activeDeviceID = null;
            _hasSession = false;
            _deviceList = [];
            _floorSeconds = 0;
        }

        Changed?.Invoke();
    }

    private void OnQueueSnapshot(ResponseQueue queue)
    {
        lock (_lock)
        {
            if (_registering)
            {
                _pendingSnapshot = queue;
                return;
            }
        }

        Enqueue(() => ApplySnapshot(queue));
    }

    private async Task ApplySnapshot(ResponseQueue queue)
    {
        await _serviceQueue.ReplaceWithSnapshot(queue, Role == PlaybackRoleEnum.Active);
    }

    private async Task RequestSnapshot()
    {
        ResponseQueue? snapshot = await _serviceMediaHub.Invoke<ResponseQueue>(MediaHubMethodEnum.RequestSnapshot, null);
        if (snapshot is not null) await ApplySnapshot(snapshot);
    }

    private void Enqueue(Func<Task> work)
    {
        lock (_lock) _tail = Chain(_tail, work);
    }

    // What failed is named by its type only: a message could hold a queue.
    private async Task Chain(Task previous, Func<Task> work)
    {
        await previous;
        try
        {
            await work();
        }
        catch (Exception exception)
        {
            _logger.LogError("A step of the live session failed ({ExceptionType}).", exception.GetType().Name);
        }
    }
}
