using System.ComponentModel;
using Microsoft.Extensions.Logging;
using Noxy.NET.MediaPlayer.Application.Requests.Remote;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Responses.Remote;
using Noxy.NET.MediaPlayer.Presentation.Enums;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;

namespace Noxy.NET.MediaPlayer.Presentation.Services;

public class RemoteCommandHandlerService(IMediaHubService serviceMediaHub, IRemotePlaybackService serviceRemote, IPlayerService servicePlayer, IQueueService serviceQueue, IPlaybackReporterService serviceReporter, ILogger<RemoteCommandHandlerService> logger)
    : IRemoteCommandHandlerService
{
    // A transfer is told apart by the state and the queue it was made from: the same push twice starts the target once.
    private sealed record StartKey(long StateRevision, long QueueRevision, Guid? CurrentItemID);

    private readonly Lock _lock = new();

    // Commands, transfers and starts are carried out one at a time, in the order they came.
    private Task _tail = Task.CompletedTask;
    private bool _started;
    private bool _pausedForTransfer;
    private bool _wasActive;
    private StartKey? _lastStartKey;
    private (Guid? ActiveDeviceID, long StateRevision)? _adoptedState;

    public void Start()
    {
        lock (_lock)
        {
            if (_started) return;

            _started = true;
        }

        serviceMediaHub.PlayerCommandReceived += command => Enqueue(() => OnCommand(command));
        serviceMediaHub.PlayerTransferRequested += () => Enqueue(OnTransferRequested);
        serviceMediaHub.PlayerStartRequested += (state, queue) => Enqueue(() => OnStartRequested(state, queue));
        serviceRemote.Changed += () => Enqueue(OnRemoteChanged);
        serviceRemote.TransferFailed += () => Enqueue(OnTransferFailed);
    }

    #region -- Commands --

    private async Task OnCommand(RequestPlayerCommand command)
    {
        // A command that reaches a device that is not the active one is a late message: it is not carried out.
        if (serviceRemote.Role != PlaybackRoleEnum.Active)
        {
            logger.LogInformation("A player command arrived at a device that is not the active one and was ignored.");
            return;
        }

        switch (command.Command)
        {
            case PlayerCommandEnum.Play:
                await servicePlayer.Resume();
                break;
            case PlayerCommandEnum.Pause:
                await servicePlayer.Pause();
                break;
            case PlayerCommandEnum.Seek:
                await servicePlayer.Seek(TimeSpan.FromSeconds(command.PositionSeconds ?? 0));
                break;
            case PlayerCommandEnum.Next:
                await servicePlayer.Next();
                break;
            case PlayerCommandEnum.Previous:
                await servicePlayer.Previous();
                break;
            case PlayerCommandEnum.JumpTo:
                if (command.ItemID is { } itemID) await servicePlayer.JumpTo(itemID, command.KeepSkipped ?? false);
                break;
            case PlayerCommandEnum.SetVolume:
                if (command.Volume is { } volume) await servicePlayer.SetVolume(Math.Clamp(volume, 0, 100) / 100.0);
                break;
            case PlayerCommandEnum.SetShuffle:
                if (command.Shuffle is { } shuffle) await servicePlayer.SetShuffle(shuffle);
                break;
            case PlayerCommandEnum.SetRepeat:
                if (command.Repeat is { } repeat) await servicePlayer.SetRepeat(repeat);
                break;
            case PlayerCommandEnum.PlayLibrary:
                if (command.MediaFileID is { } mediaFileID) await servicePlayer.StartLibrary(mediaFileID, command.Sort ?? LibrarySortEnum.Title, command.Descending ?? false, command.Search ?? string.Empty);
                break;
            case PlayerCommandEnum.PlayPlaylist:
                if (command.PlaylistID is { } playlistID) await servicePlayer.StartPlaylist(playlistID, command.EntryID);
                break;
            default:
                throw new InvalidEnumArgumentException(nameof(command), (int)command.Command, typeof(PlayerCommandEnum));
        }

        await serviceReporter.ReportNow();
    }

    #endregion -- Commands --

    #region -- Transfer --

    // Another device is taking over: this one stops at once and says where it stopped.
    private async Task OnTransferRequested()
    {
        if (serviceRemote.Role != PlaybackRoleEnum.Active)
        {
            logger.LogInformation("A transfer was asked of a device that is not the active one and was ignored.");
            return;
        }

        bool wasPlaying = servicePlayer.IsPlaying;
        await servicePlayer.Pause();
        lock (_lock) _pausedForTransfer = wasPlaying;

        await serviceReporter.ReportNow();
    }

    // The transfer did not work: the device that stopped for it plays on.
    private async Task OnTransferFailed()
    {
        bool resume;
        lock (_lock)
        {
            resume = _pausedForTransfer;
            _pausedForTransfer = false;
        }

        if (!resume || serviceRemote.Role != PlaybackRoleEnum.Active) return;

        await servicePlayer.Resume();
        await serviceReporter.ReportNow();
    }

    // The target of a transfer: takes the queue, plays it from where the old device stopped (a media file that is not here is downloaded first), and then claims the active role.
    private async Task OnStartRequested(ResponsePlayerState state, ResponseQueue queue)
    {
        StartKey key = new(state.StateRevision, queue.QueueRevision, queue.Current?.ItemID);
        lock (_lock)
        {
            if (_lastStartKey == key) return;
        }

        // An empty queue has nothing to play: nothing starts and nothing is claimed.
        if (queue.Current is null) return;

        await serviceQueue.ReplaceWithSnapshot(queue, false);
        await servicePlayer.SetVolume(Math.Clamp(state.Volume, 0, 100) / 100.0);
        await servicePlayer.PlayCurrent();

        // A start that did not get to play (a file that would not load) is not remembered: the same request may be tried again.
        if (!servicePlayer.IsPlaying) return;

        lock (_lock) _lastStartKey = key;

        if (state.PositionSeconds > 0) await servicePlayer.Seek(TimeSpan.FromSeconds(state.PositionSeconds));

        // The reporter stops the player when the server refuses; the target of a transfer makes sure of it too, so two devices never play together.
        if (await serviceReporter.ClaimNow() is false) await servicePlayer.Pause();
    }

    #endregion -- Transfer --

    #region -- The live session --

    // A device that is not the active one adopts the volume of each state push, once. A device that was the active one and no longer is (a transfer that failed late is one way it happens) stops if it still plays: only the active device plays.
    private async Task OnRemoteChanged()
    {
        PlaybackRoleEnum role = serviceRemote.Role;
        bool lostTheRole;
        lock (_lock)
        {
            lostTheRole = _wasActive && role == PlaybackRoleEnum.Controller;
            _wasActive = role == PlaybackRoleEnum.Active;
            if (role != PlaybackRoleEnum.Active) _pausedForTransfer = false;
        }

        if (lostTheRole && servicePlayer.IsPlaying)
        {
            logger.LogInformation("A device that lost the active role was playing and was stopped.");
            await servicePlayer.Pause();
        }

        if (role != PlaybackRoleEnum.Controller || serviceRemote.Remote?.State is not { } state) return;

        lock (_lock)
        {
            if (_adoptedState == (state.ActiveDeviceID, state.StateRevision)) return;

            _adoptedState = (state.ActiveDeviceID, state.StateRevision);
        }

        if ((int)Math.Round(servicePlayer.Volume * 100) != state.Volume) await servicePlayer.SetVolume(Math.Clamp(state.Volume, 0, 100) / 100.0);
    }

    #endregion -- The live session --

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
            logger.LogWarning("A request of the live session could not be carried out ({ExceptionType}).", exception.GetType().Name);
        }
    }
}
