using System.ComponentModel;
using Microsoft.Extensions.Logging;
using Noxy.NET.MediaPlayer.Application.Requests.Remote;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Models;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Presentation.Enums;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;

namespace Noxy.NET.MediaPlayer.Presentation.Services;

public class PlaybackReporterService(IPlayerService servicePlayer, IQueueService serviceQueue, IRemotePlaybackService serviceRemote, IMediaHubService serviceMediaHub, IMetadataStore storeMetadata, IDelay delay, ILogger<PlaybackReporterService> logger)
    : IPlaybackReporterService
{
    // What a report is sent for: when any of it changes. The position is not here, so a position that only moves on does not report.
    private sealed record Signature(bool IsPlaying, Guid? ItemID, int Volume, ShuffleModeEnum Shuffle, RepeatModeEnum Repeat);

    private const int MaxTextLength = 500;

    private readonly Lock _lock = new();

    // Everything this device sends goes through one chain, so a report, a claim and a queue edit reach the server in the order they were made and never at the same time.
    private Task _tail = Task.CompletedTask;
    private bool _started;
    private bool _reportQueued;
    private bool _claimInFlight;
    private long _stateRevision;
    private Signature? _last;
    private CancellationTokenSource? _saveSource;

    // Edits applied while a claim is on its way: they follow the claim when it is accepted, and are dropped when it is refused.
    private readonly List<QueueEdit> _heldEditList = [];

    public void Start()
    {
        lock (_lock)
        {
            if (_started) return;

            _started = true;
        }

        serviceQueue.OwnsPlayback = serviceRemote.Role != PlaybackRoleEnum.Controller;
        servicePlayer.Changed += OnPlayerChanged;
        serviceRemote.Changed += OnRemoteChanged;
        serviceRemote.Registered += OnRegistered;
        serviceQueue.EditApplied += OnEditApplied;
    }

    public Task ReportNow()
    {
        return Enqueue(SendReport);
    }

    public async Task<bool?> ClaimNow()
    {
        lock (_lock) _claimInFlight = true;

        bool? accepted = null;
        await Enqueue(async () =>
        {
            try
            {
                accepted = await Claim();
            }
            finally
            {
                lock (_lock)
                {
                    _claimInFlight = false;
                    _heldEditList.Clear();
                }
            }
        });

        return accepted;
    }

    #region -- Triggers --

    private void OnPlayerChanged()
    {
        UpdateSaveLoop(servicePlayer.IsPlaying);

        PlaybackRoleEnum role = serviceRemote.Role;
        switch (role)
        {
            case PlaybackRoleEnum.Active:
            {
                bool changed;
                lock (_lock) changed = _last is null || _last != Capture() || servicePlayer.PositionJumped;
                if (changed) QueueReport();
                break;
            }
            case PlaybackRoleEnum.Local when serviceRemote.IsOnline && servicePlayer.IsPlaying && servicePlayer.Current is not null:
                QueueClaim(false);
                break;
            case PlaybackRoleEnum.Controller:
                break;
            default:
                throw new InvalidEnumArgumentException(nameof(role), (int)role, typeof(PlaybackRoleEnum));
        }
    }

    // The queue may only change underneath this device (a media file deleted, a playlist edited) while it plays.
    private void OnRemoteChanged()
    {
        serviceQueue.OwnsPlayback = serviceRemote.Role != PlaybackRoleEnum.Controller;
    }

    // A device that was playing when it (re)connected asks for the active role; one that is not playing has only registered.
    private void OnRegistered()
    {
        serviceQueue.OwnsPlayback = serviceRemote.Role != PlaybackRoleEnum.Controller;
        if (serviceRemote.IsOnline && servicePlayer.IsPlaying && servicePlayer.Current is not null) QueueClaim(true);
    }

    private void OnEditApplied(QueueEdit edit)
    {
        PlaybackRoleEnum role = serviceRemote.Role;
        lock (_lock)
        {
            if (_claimInFlight && role != PlaybackRoleEnum.Active)
            {
                _heldEditList.Add(edit);
                return;
            }
        }

        if (role == PlaybackRoleEnum.Active) _ = Enqueue(() => serviceMediaHub.Send(MediaHubMethodEnum.EditQueue, edit));
    }

    #endregion -- Triggers --

    #region -- Sending --

    // A report waiting for its turn reads the state when it runs, so a burst of changes while one is in flight costs one more report, never one each.
    private void QueueReport()
    {
        lock (_lock)
        {
            if (_reportQueued) return;

            _reportQueued = true;
        }

        _ = Enqueue(SendReport);
    }

    // A device that starts to play on its own claims only when nobody is active. One that was playing when it (re)connected claims whatever registering said, also when it is the active device itself (it came back within the grace period): its queue may have moved on while it was away. It is refused when another device really plays.
    private void QueueClaim(bool onRegistration)
    {
        lock (_lock)
        {
            if (_claimInFlight) return;

            _claimInFlight = true;
        }

        _ = Enqueue(async () =>
        {
            try
            {
                PlaybackRoleEnum role = serviceRemote.Role;
                if ((!onRegistration && role != PlaybackRoleEnum.Local) || !serviceRemote.IsOnline || !servicePlayer.IsPlaying) return;

                await Claim();
            }
            finally
            {
                lock (_lock)
                {
                    _claimInFlight = false;
                    _heldEditList.Clear();
                }
            }
        });
    }

    private async Task SendReport()
    {
        lock (_lock) _reportQueued = false;
        if (serviceRemote.Role != PlaybackRoleEnum.Active) return;

        long revision;
        lock (_lock) revision = ++_stateRevision;

        await serviceMediaHub.Send(MediaHubMethodEnum.ReportState, await BuildState(revision));
    }

    private async Task<bool?> Claim()
    {
        lock (_lock)
        {
            // What was applied up to now is in the queue that is sent.
            _heldEditList.Clear();
        }

        RequestClaimActive request = new() { State = await BuildState(1), Queue = serviceQueue.Queue.ToResponse() };
        bool? accepted = await serviceMediaHub.Invoke<bool?>(MediaHubMethodEnum.ClaimActive, request);

        if (accepted is true)
        {
            List<QueueEdit> heldList;
            lock (_lock)
            {
                _stateRevision = 1;
                heldList = [.. _heldEditList];
                _heldEditList.Clear();
            }

            serviceRemote.MarkActive();
            foreach (QueueEdit edit in heldList) await serviceMediaHub.Send(MediaHubMethodEnum.EditQueue, edit);
        }
        else if (accepted is false)
        {
            // Another device plays: this one stops in the same breath, so two never play together.
            await servicePlayer.Pause();
        }

        return accepted;
    }

    private async Task<RequestReportPlayerState> BuildState(long revision)
    {
        QueueItem? current = servicePlayer.Current;
        ResponseMediaFile? mediaFile = current is null ? null : await storeMetadata.GetMediaFile(current.MediaFileID);
        Signature signature = Capture();
        lock (_lock) _last = signature;

        servicePlayer.PositionJumped = false;

        return new()
        {
            StateRevision = revision,
            IsPlaying = signature.IsPlaying,
            PositionSeconds = Math.Max(0, servicePlayer.Position.TotalSeconds),
            Volume = signature.Volume,
            Shuffle = signature.Shuffle,
            Repeat = signature.Repeat,
            CurrentItemID = signature.ItemID,
            Title = Truncate(mediaFile?.Title),
            Artist = Truncate(mediaFile?.Artist)
        };
    }

    private Signature Capture()
    {
        return new(servicePlayer.IsPlaying, servicePlayer.Current?.ItemID, (int)Math.Round(Math.Clamp(servicePlayer.Volume, 0, 1) * 100), servicePlayer.Shuffle, servicePlayer.Repeat);
    }

    private static string? Truncate(string? text)
    {
        return text is { Length: > MaxTextLength } ? text[..MaxTextLength] : text;
    }

    // What failed is named by its type only: a message could hold a queue or a token.
    private Task Enqueue(Func<Task> work)
    {
        lock (_lock) return _tail = Chain(_tail, work);
    }

    private async Task Chain(Task previous, Func<Task> work)
    {
        await previous;
        try
        {
            await work();
        }
        catch (Exception exception)
        {
            logger.LogWarning("A message to the server could not be sent ({ExceptionType}).", exception.GetType().Name);
        }
    }

    #endregion -- Sending --

    #region -- The saved position --

    // While playing, the position of the local copy is saved now and then, and once more when playing stops.
    private void UpdateSaveLoop(bool isPlaying)
    {
        CancellationTokenSource? stopped = null;
        CancellationTokenSource? started = null;
        lock (_lock)
        {
            if (isPlaying && _saveSource is null)
            {
                started = _saveSource = new();
            }
            else if (!isPlaying && _saveSource is not null)
            {
                stopped = _saveSource;
                _saveSource = null;
            }
        }

        if (started is not null) _ = SaveLoop(started.Token);

        if (stopped is null) return;

        stopped.Cancel();
        stopped.Dispose();
        _ = SaveNow();
    }

    private async Task SaveLoop(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await delay.Wait(TimeSpan.FromSeconds(Constants.LocalQueueSaveSeconds), cancellationToken);
                await serviceQueue.SaveLocalCopy(servicePlayer.Position.TotalSeconds);
            }
        }
        catch (OperationCanceledException)
        {
            // Playing stopped.
        }
        catch (Exception exception)
        {
            logger.LogWarning("The local copy of the queue could not be saved ({ExceptionType}).", exception.GetType().Name);
        }
    }

    private async Task SaveNow()
    {
        try
        {
            await serviceQueue.SaveLocalCopy(servicePlayer.Position.TotalSeconds);
        }
        catch (Exception exception)
        {
            logger.LogWarning("The local copy of the queue could not be saved ({ExceptionType}).", exception.GetType().Name);
        }
    }

    #endregion -- The saved position --
}
