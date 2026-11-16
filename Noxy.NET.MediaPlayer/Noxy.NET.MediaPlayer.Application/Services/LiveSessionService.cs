using System.ComponentModel;
using Mediator;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Noxy.NET.MediaPlayer.Application.Configurations;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Application.Models.Media;
using Noxy.NET.MediaPlayer.Application.Models.Remote;
using Noxy.NET.MediaPlayer.Application.Notifications.Remote;
using Noxy.NET.MediaPlayer.Application.Requests.Remote;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Models;
using Noxy.NET.MediaPlayer.Domain.Responses.Remote;

namespace Noxy.NET.MediaPlayer.Application.Services;

public class LiveSessionService : ILiveSessionService
{
    private readonly IDeviceRegistryService _serviceDeviceRegistry;
    private readonly IOptions<ConfigRemote> _configRemote;
    private readonly TimeProvider _time;
    private readonly IPublisher _publisher;
    private readonly IUnitOfWorkFactory _factoryUnitOfWork;
    private readonly ILogger<LiveSessionService> _logger;
    private readonly Lock _lock = new();
    private readonly Dictionary<Guid, LiveSession> _sessionMap = [];

    public LiveSessionService(IDeviceRegistryService serviceDeviceRegistry, IOptions<ConfigRemote> configRemote, TimeProvider time, IPublisher publisher, IUnitOfWorkFactory factoryUnitOfWork, ILogger<LiveSessionService> logger)
    {
        _serviceDeviceRegistry = serviceDeviceRegistry;
        _configRemote = configRemote;
        _time = time;
        _publisher = publisher;
        _factoryUnitOfWork = factoryUnitOfWork;
        _logger = logger;

        _serviceDeviceRegistry.DeviceDisconnected += OnDeviceDisconnected;
        _serviceDeviceRegistry.ActiveDeviceResolver = GetActiveDeviceID;
    }

    private ConfigRemote Config => _configRemote.Value;

    #region -- Claim and report --

    public async Task<bool> Claim(Guid userID, Guid deviceID, RequestClaimActive request)
    {
        if (!_serviceDeviceRegistry.IsOnline(userID, deviceID)) return false;

        ResponseQueue sent = request.Queue;
        if (sent.HistoryList is null || sent.UserQueueList is null || sent.SourceList is null) throw new InvalidQueueEditException("The queue lists are missing.");

        if (sent.HistoryList.Count + sent.UserQueueList.Count + sent.SourceList.Count + (sent.Current is null ? 0 : 1) > Config.MaxQueueLength) throw new InvalidQueueEditException("The queue is too long.");

        LiveQueue queue = LiveQueue.From(sent, Config.HistoryLimit);
        List<object> notificationList;
        lock (_lock)
        {
            _sessionMap.TryGetValue(userID, out LiveSession? session);
            // The device a transfer is moving playback to is always welcome, whatever a late report says about the old one.
            bool isTransferTarget = session is { TransferPending: true } && session.TransferTargetID == deviceID;
            if (session?.ActiveDeviceID is { } active && active != deviceID && !isTransferTarget && session.IsPlaying && _serviceDeviceRegistry.IsOnline(userID, active)) return false;

            if (session is null)
            {
                session = new() { UserID = userID, Queue = queue };
                _sessionMap[userID] = session;
            }
            else
            {
                // The revision keeps rising across claims, so a client that held the old queue can tell it was replaced.
                queue.QueueRevision = session.Queue.QueueRevision + 1;
                session.Queue = queue;
            }

            CancelGrace(session);
            session.ActiveDeviceID = deviceID;
            Store(session, request.State);
            RefreshIdle(session);
            if (session.TransferPending && session.TransferTargetID == deviceID) session.ClaimSource?.TrySetResult();

            notificationList =
            [
                new NotificationPlayerStateChanged(userID, ToState(session)),
                new NotificationQueueSnapshot(userID, queue.ToResponse()),
                new NotificationDeviceListChanged(userID, _serviceDeviceRegistry.GetDeviceList(userID, deviceID))
            ];
        }

        await PublishAll(notificationList);
        return true;
    }

    public async Task<bool> Report(Guid userID, Guid deviceID, RequestReportPlayerState state)
    {
        NotificationPlayerStateChanged notification;
        lock (_lock)
        {
            if (!_sessionMap.TryGetValue(userID, out LiveSession? session) || session.ActiveDeviceID != deviceID || state.StateRevision <= session.StateRevision) return false;

            // A report from the device itself means it is back.
            CancelGrace(session);
            Store(session, state);
            RefreshIdle(session);
            // Only a paused report is the final state that was asked for; a routine report of a device still playing is not.
            if (session.TransferPending && !state.IsPlaying) session.FinalStateSource?.TrySetResult();

            notification = new(userID, ToState(session));
        }

        await _publisher.Publish(notification);
        return true;
    }

    #endregion -- Claim and report --

    #region -- Edit --

    public async Task<ResponseQueueDelta?> Edit(Guid userID, Guid deviceID, QueueEdit edit, CancellationToken cancellationToken = default)
    {
        HashSet<Guid> knownMediaFileIDSet;
        lock (_lock)
        {
            if (!_sessionMap.TryGetValue(userID, out LiveSession? session)) return null;

            if (IsActiveOnly(edit.Edit) && session.ActiveDeviceID != deviceID) throw new InvalidQueueEditException("Only the active device can make this edit.");

            ResponseQueue current = session.Queue.ToResponse();
            knownMediaFileIDSet = [.. current.HistoryList.Concat(current.UserQueueList).Concat(current.SourceList).Concat(current.Current is null ? [] : [current.Current]).Select(x => x.MediaFileID)];
        }

        await EnsureMediaFilesBelongToUser(userID, edit, knownMediaFileIDSet, cancellationToken);

        ResponseQueueDelta? delta;
        lock (_lock)
        {
            // The session may have ended while the media files were checked.
            if (!_sessionMap.TryGetValue(userID, out LiveSession? session)) return null;

            // The active device may have changed while the media files were checked.
            if (IsActiveOnly(edit.Edit) && session.ActiveDeviceID != deviceID) throw new InvalidQueueEditException("Only the active device can make this edit.");

            EnsureRoom(session.Queue, edit);
            delta = session.Queue.Apply(edit, Config.HistoryLimit);
            RefreshIdle(session);
        }

        if (delta is null) return null;

        await _publisher.Publish(new NotificationQueueChanged(userID, delta), cancellationToken);
        return delta;
    }

    // The kinds that change the source part, or report what the device is playing, belong to the device that plays.
    private static bool IsActiveOnly(QueueEditEnum edit)
    {
        return edit switch
        {
            QueueEditEnum.ReplaceSource or QueueEditEnum.RegenerateSource or QueueEditEnum.InsertSourceItem or QueueEditEnum.RemoveSourceItem or QueueEditEnum.Advance or QueueEditEnum.Previous => true,
            QueueEditEnum.PlayNext or QueueEditEnum.AddToQueue or QueueEditEnum.Remove or QueueEditEnum.Move => false,
            _ => throw new InvalidEnumArgumentException(nameof(edit), (int)edit, typeof(QueueEditEnum))
        };
    }

    // A media file the queue does not hold yet must be one of the user's.
    private async Task EnsureMediaFilesBelongToUser(Guid userID, QueueEdit edit, HashSet<Guid> knownMediaFileIDSet, CancellationToken cancellationToken)
    {
        List<Guid> neededList = [];
        if (edit.MediaFileID is { } mediaFileID) neededList.Add(mediaFileID);
        if (edit.Item is { } item) neededList.Add(item.MediaFileID);
        if (edit.ItemList is { } itemList) neededList.AddRange(itemList.Select(x => x.MediaFileID));

        neededList = [.. neededList.Where(x => !knownMediaFileIDSet.Contains(x)).Distinct()];
        if (neededList.Count == 0) return;

        await using IUnitOfWork uow = _factoryUnitOfWork.Create();
        List<MediaFileStamp> stampList = await uow.Media.GetMediaFileStampList(userID, cancellationToken);
        HashSet<Guid> ownedSet = [.. stampList.Select(x => x.ID)];
        if (neededList.Any(x => !ownedSet.Contains(x))) throw new MediaNotFoundException();
    }

    private void EnsureRoom(LiveQueue queue, QueueEdit edit)
    {
        switch (edit.Edit)
        {
            case QueueEditEnum.PlayNext:
            case QueueEditEnum.AddToQueue:
            case QueueEditEnum.InsertSourceItem:
                if (queue.ItemCount >= Config.MaxQueueLength) throw new InvalidQueueEditException("The queue is full.");

                break;
            case QueueEditEnum.ReplaceSource:
                if (edit.ItemList is { } replaceList && replaceList.Count + (edit.KeepUserQueue ? queue.UserQueueCount : 0) > Config.MaxQueueLength) throw new InvalidQueueEditException("The queue is too long.");

                break;
            case QueueEditEnum.RegenerateSource:
                if (edit.ItemList is { } regenerateList && queue.ItemCount - queue.SourceCount + regenerateList.Count > Config.MaxQueueLength) throw new InvalidQueueEditException("The queue is too long.");

                break;
            case QueueEditEnum.Remove:
            case QueueEditEnum.Move:
            case QueueEditEnum.RemoveSourceItem:
            case QueueEditEnum.Advance:
            case QueueEditEnum.Previous:
                break;
            default:
                throw new InvalidEnumArgumentException(nameof(edit), (int)edit.Edit, typeof(QueueEditEnum));
        }
    }

    #endregion -- Edit --

    #region -- Reading --

    public ResponseQueue? GetQueue(Guid userID)
    {
        lock (_lock) return _sessionMap.TryGetValue(userID, out LiveSession? session) ? session.Queue.ToResponse() : null;
    }

    public ResponsePlayerState? GetState(Guid userID)
    {
        lock (_lock) return _sessionMap.TryGetValue(userID, out LiveSession? session) ? ToState(session) : null;
    }

    public Guid? GetActiveDeviceID(Guid userID)
    {
        lock (_lock) return _sessionMap.TryGetValue(userID, out LiveSession? session) ? session.ActiveDeviceID : null;
    }

    public void Touch(Guid userID)
    {
        lock (_lock)
        {
            if (_sessionMap.TryGetValue(userID, out LiveSession? session)) RefreshIdle(session);
        }
    }

    #endregion -- Reading --

    #region -- Transfer --

    public Task<bool> Transfer(Guid userID, Guid targetDeviceID, Func<Guid, Task> requestFinalState, Func<Guid, ResponsePlayerState, ResponseQueue, Task> startTarget)
    {
        TaskCompletionSource finalStateSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource claimSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        Guid activeDeviceID;
        lock (_lock)
        {
            if (!_sessionMap.TryGetValue(userID, out LiveSession? session) || session.ActiveDeviceID is not { } active) return Task.FromResult(false);
            if (session.TransferPending || targetDeviceID == active || !_serviceDeviceRegistry.IsOnline(userID, targetDeviceID)) return Task.FromResult(false);

            session.TransferPending = true;
            session.TransferTargetID = targetDeviceID;
            session.FinalStateSource = finalStateSource;
            session.ClaimSource = claimSource;
            activeDeviceID = active;
        }

        // The caller is usually the active device itself, so the transfer must not hold its hub connection: it runs on while the answers arrive.
        _ = RunGuarded(() => RunTransfer(userID, activeDeviceID, targetDeviceID, finalStateSource, claimSource, requestFinalState, startTarget));
        return Task.FromResult(true);
    }

    private async Task RunTransfer(Guid userID, Guid activeDeviceID, Guid targetDeviceID, TaskCompletionSource finalStateSource, TaskCompletionSource claimSource, Func<Guid, Task> requestFinalState, Func<Guid, ResponsePlayerState, ResponseQueue, Task> startTarget)
    {
        TimeSpan timeout = TimeSpan.FromSeconds(Config.TransferTimeoutSeconds);
        bool claimed = false;
        try
        {
            // An old device that cannot be reached counts as one that never answered: its last state is used.
            bool answered = false;
            try
            {
                await requestFinalState(activeDeviceID);
                answered = await WaitFor(finalStateSource.Task, timeout);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "Could not ask the active device for its final state");
            }

            ResponsePlayerState state;
            ResponseQueue queue;
            lock (_lock)
            {
                if (!_sessionMap.TryGetValue(userID, out LiveSession? session)) return;

                // An old device that never answered: its last state counts, advanced by the time that passed when it was playing, and it no longer counts as playing.
                if (!answered && session.IsPlaying)
                {
                    DateTime now = _time.GetUtcNow().UtcDateTime;
                    session.PositionSeconds += Math.Max(0, (now - session.StateTime).TotalSeconds);
                    session.StateTime = now;
                    session.IsPlaying = false;
                }

                state = ToState(session);
                queue = session.Queue.ToResponse();
            }

            await startTarget(targetDeviceID, state, queue);
            claimed = await WaitFor(claimSource.Task, timeout);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // The target could not be started (it went offline): the transfer failed and the old device stays active.
            _logger.LogWarning(ex, "A transfer failed");
        }
        finally
        {
            lock (_lock)
            {
                if (_sessionMap.TryGetValue(userID, out LiveSession? session))
                {
                    session.TransferPending = false;
                    session.TransferTargetID = null;
                    session.FinalStateSource = null;
                    session.ClaimSource = null;
                }
            }
        }

        if (!claimed) await _publisher.Publish(new NotificationTransferFailed(userID));
    }

    private async Task<bool> WaitFor(Task task, TimeSpan timeout)
    {
        try
        {
            await task.WaitAsync(timeout, _time);
            return true;
        }
        catch (TimeoutException)
        {
            return false;
        }
    }

    #endregion -- Transfer --

    #region -- Dropped active device and idle teardown --

    private Task OnDeviceDisconnected(Guid userID, Guid deviceID)
    {
        lock (_lock)
        {
            if (!_sessionMap.TryGetValue(userID, out LiveSession? session) || session.ActiveDeviceID != deviceID) return Task.CompletedTask;

            // The device keeps its place for the grace period: a short drop must not lose it.
            CancelGrace(session);
            long version = ++session.GraceVersion;
            session.GraceTimer = _time.CreateTimer(_ => _ = RunGuarded(() => GraceElapsed(userID, deviceID, version)), null, TimeSpan.FromSeconds(Config.ActiveGraceSeconds), Timeout.InfiniteTimeSpan);
        }

        return Task.CompletedTask;
    }

    private async Task GraceElapsed(Guid userID, Guid deviceID, long version)
    {
        List<object> notificationList;
        lock (_lock)
        {
            if (!_sessionMap.TryGetValue(userID, out LiveSession? session) || session.GraceVersion != version || session.ActiveDeviceID != deviceID) return;

            session.GraceTimer = null;
            if (_serviceDeviceRegistry.IsOnline(userID, deviceID)) return;

            // The device is gone: the queue stays, nothing plays, and the idle countdown starts.
            DateTime now = _time.GetUtcNow().UtcDateTime;
            if (session.IsPlaying) session.PositionSeconds += Math.Max(0, (now - session.StateTime).TotalSeconds);

            session.StateTime = now;
            session.IsPlaying = false;
            session.ActiveDeviceID = null;
            session.StateRevision = 0;
            RefreshIdle(session);
            notificationList = [new NotificationPlayerStateChanged(userID, ToState(session)), new NotificationDeviceListChanged(userID, _serviceDeviceRegistry.GetDeviceList(userID, null))];
        }

        await PublishAll(notificationList);
    }

    private async Task IdleElapsed(Guid userID, long version)
    {
        lock (_lock)
        {
            if (!_sessionMap.TryGetValue(userID, out LiveSession? session) || session.IdleVersion != version) return;

            // Safety net: a session whose active device is online and playing never ends.
            if (session.ActiveDeviceID is not null && session.IsPlaying) return;

            CancelGrace(session);
            session.IdleTimer?.Dispose();
            _sessionMap.Remove(userID);
        }

        await _publisher.Publish(new NotificationSessionEnded(userID));
    }

    // The countdown never runs while the active device is online and playing; otherwise it runs from the last activity.
    private void RefreshIdle(LiveSession session)
    {
        session.IdleTimer?.Dispose();
        session.IdleTimer = null;
        long version = ++session.IdleVersion;
        if (session.ActiveDeviceID is not null && session.IsPlaying) return;

        Guid userID = session.UserID;
        session.IdleTimer = _time.CreateTimer(_ => _ = RunGuarded(() => IdleElapsed(userID, version)), null, TimeSpan.FromMinutes(Config.IdleTeardownMinutes), Timeout.InfiniteTimeSpan);
    }

    private static void CancelGrace(LiveSession session)
    {
        session.GraceTimer?.Dispose();
        session.GraceTimer = null;
        session.GraceVersion++;
    }

    // Timer callbacks have nobody to throw to, so a failure is logged.
    private async Task RunGuarded(Func<Task> work)
    {
        try
        {
            await work();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "A live session timer failed");
        }
    }

    #endregion -- Dropped active device and idle teardown --

    #region -- State --

    private void Store(LiveSession session, RequestReportPlayerState state)
    {
        session.StateRevision = state.StateRevision;
        session.IsPlaying = state.IsPlaying;
        session.PositionSeconds = state.PositionSeconds;
        session.StateTime = _time.GetUtcNow().UtcDateTime;
        session.Volume = state.Volume;
        session.Shuffle = state.Shuffle;
        session.Repeat = state.Repeat;
        session.CurrentItemID = state.CurrentItemID;
        session.Title = state.Title;
        session.Artist = state.Artist;
    }

    private static ResponsePlayerState ToState(LiveSession session)
    {
        return new()
        {
            ActiveDeviceID = session.ActiveDeviceID,
            StateRevision = session.StateRevision,
            IsPlaying = session.IsPlaying,
            PositionSeconds = session.PositionSeconds,
            ServerTime = session.StateTime,
            Volume = session.Volume,
            Shuffle = session.Shuffle,
            Repeat = session.Repeat,
            CurrentItemID = session.CurrentItemID,
            Title = session.Title,
            Artist = session.Artist
        };
    }

    private async Task PublishAll(List<object> notificationList)
    {
        foreach (object notification in notificationList)
        {
            await _publisher.Publish(notification);
        }
    }

    #endregion -- State --
}
