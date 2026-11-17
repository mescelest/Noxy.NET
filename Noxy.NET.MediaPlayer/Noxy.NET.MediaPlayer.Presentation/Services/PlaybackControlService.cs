using Microsoft.Extensions.Logging;
using Noxy.NET.MediaPlayer.Application.Requests.Remote;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Models;
using Noxy.NET.MediaPlayer.Presentation.Enums;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;

namespace Noxy.NET.MediaPlayer.Presentation.Services;

public class PlaybackControlService : IPlaybackControlService
{
    private readonly IPlayerService _servicePlayer;
    private readonly IQueueService _serviceQueue;
    private readonly IRemotePlaybackService _serviceRemote;
    private readonly IMediaHubService _serviceMediaHub;
    private readonly IPlaybackReporterService _serviceReporter;
    private readonly IDeviceService _serviceDevice;
    private readonly IDelay _delay;
    private readonly ILogger<PlaybackControlService> _logger;
    private CancellationTokenSource? _seekSource;
    private CancellationTokenSource? _volumeSource;

    public PlaybackControlService(IPlayerService servicePlayer, IQueueService serviceQueue, IRemotePlaybackService serviceRemote, IMediaHubService serviceMediaHub, IPlaybackReporterService serviceReporter, IDeviceService serviceDevice, IDelay delay,
        ILogger<PlaybackControlService> logger)
    {
        _servicePlayer = servicePlayer;
        _serviceQueue = serviceQueue;
        _serviceRemote = serviceRemote;
        _serviceMediaHub = serviceMediaHub;
        _serviceReporter = serviceReporter;
        _serviceDevice = serviceDevice;
        _delay = delay;
        _logger = logger;

        serviceRemote.TransferFailed += () => TransferFailed?.Invoke();
    }

    public event Action? TransferFailed;

    public bool CanContinue => _serviceRemote.Role != PlaybackRoleEnum.Controller && !_servicePlayer.IsPlaying && _serviceQueue.Queue.Current is not null;

    private bool IsController => _serviceRemote.Role == PlaybackRoleEnum.Controller;

    #region -- Playing --

    public async Task Play()
    {
        await Route(new() { Command = PlayerCommandEnum.Play }, async () =>
        {
            await _servicePlayer.Resume();

            // Nothing is loaded yet (a queue that was restored from the local copy): it starts now.
            if (!_servicePlayer.IsPlaying && !_servicePlayer.IsDisabled) await _servicePlayer.PlayCurrent();
        });
    }

    public async Task Pause()
    {
        await Route(new() { Command = PlayerCommandEnum.Pause }, _servicePlayer.Pause);
    }

    public async Task Next()
    {
        await Route(new() { Command = PlayerCommandEnum.Next }, _servicePlayer.Next);
    }

    public async Task Previous()
    {
        await Route(new() { Command = PlayerCommandEnum.Previous }, _servicePlayer.Previous);
    }

    public async Task SetShuffle(ShuffleModeEnum shuffle)
    {
        await Route(new() { Command = PlayerCommandEnum.SetShuffle, Shuffle = shuffle }, () => _servicePlayer.SetShuffle(shuffle));
    }

    public async Task SetRepeat(RepeatModeEnum repeat)
    {
        await Route(new() { Command = PlayerCommandEnum.SetRepeat, Repeat = repeat }, () => _servicePlayer.SetRepeat(repeat));
    }

    public async Task JumpTo(Guid itemID, bool keepSkipped)
    {
        await Route(new() { Command = PlayerCommandEnum.JumpTo, ItemID = itemID, KeepSkipped = keepSkipped }, () => _servicePlayer.JumpTo(itemID, keepSkipped));
    }

    public async Task StartLibrary(Guid mediaFileID, LibrarySortEnum sort, bool descending, string search)
    {
        await Route(new() { Command = PlayerCommandEnum.PlayLibrary, MediaFileID = mediaFileID, Sort = sort, Descending = descending, Search = search }, () => _servicePlayer.StartLibrary(mediaFileID, sort, descending, search));
    }

    public async Task StartPlaylist(Guid playlistID, Guid? entryID = null)
    {
        await Route(new() { Command = PlayerCommandEnum.PlayPlaylist, PlaylistID = playlistID, EntryID = entryID }, () => _servicePlayer.StartPlaylist(playlistID, entryID));
    }

    #endregion -- Playing --

    #region -- Seek and volume --

    public async Task Seek(TimeSpan position)
    {
        if (!IsController)
        {
            await _servicePlayer.Seek(position);
            return;
        }

        if (await Deferred(source => Interlocked.Exchange(ref _seekSource, source)))
        {
            await Route(new() { Command = PlayerCommandEnum.Seek, PositionSeconds = Math.Max(0, position.TotalSeconds) }, () => _servicePlayer.Seek(position));
        }
    }

    public async Task SetVolume(int volume)
    {
        int clamped = Math.Clamp(volume, 0, 100);
        if (!IsController)
        {
            await _servicePlayer.SetVolume(clamped / 100.0);
            return;
        }

        if (await Deferred(source => Interlocked.Exchange(ref _volumeSource, source)))
        {
            await Route(new() { Command = PlayerCommandEnum.SetVolume, Volume = clamped }, () => _servicePlayer.SetVolume(clamped / 100.0));
        }
    }

    // Waits out the pause after the last change of a drag: a change that comes in the meantime takes over (the earlier call ends, sending nothing). True for the call that is still the latest when the pause is over.
    private async Task<bool> Deferred(Func<CancellationTokenSource, CancellationTokenSource?> swap)
    {
        // A source holds no timer or handle, so none is disposed: a later call cancels an earlier one that may still be reading its token.
        CancellationTokenSource source = new();
        CancellationToken token = source.Token;

        CancellationTokenSource? previous = swap(source);

        if (previous is not null) await previous.CancelAsync();

        try
        {
            await _delay.Wait(TimeSpan.FromMilliseconds(Constants.RemoteDeferMilliseconds), token);
        }
        catch (OperationCanceledException)
        {
            return false;
        }

        return true;
    }

    #endregion -- Seek and volume --

    #region -- The queue --

    public async Task PlayNext(Guid mediaFileID)
    {
        if (_serviceQueue.Queue.ItemCount == 0) return;

        await Edit(new() { Edit = QueueEditEnum.PlayNext, ItemID = Guid.NewGuid(), MediaFileID = mediaFileID });
    }

    public async Task AddToQueue(Guid mediaFileID)
    {
        if (_serviceQueue.Queue.ItemCount == 0) return;

        await Edit(new() { Edit = QueueEditEnum.AddToQueue, ItemID = Guid.NewGuid(), MediaFileID = mediaFileID });
    }

    public async Task RemoveFromQueue(Guid itemID)
    {
        await Edit(new() { Edit = QueueEditEnum.Remove, ItemID = itemID });
    }

    public async Task MoveInQueue(Guid itemID, Guid? afterItemID)
    {
        await Edit(new() { Edit = QueueEditEnum.Move, ItemID = itemID, AfterItemID = afterItemID });
    }

    // A controller sends the edit and waits for the server; nothing changes here until the pushed result arrives. On the active device the reporter sends what the queue applied.
    private async Task Edit(QueueEdit edit)
    {
        if (IsController)
        {
            await _serviceMediaHub.Send(MediaHubMethodEnum.EditQueue, edit);
            return;
        }

        await _serviceQueue.Apply(edit);
    }

    #endregion -- The queue --

    #region -- Devices and continuing --

    public async Task Transfer(Guid deviceID)
    {
        if (!_serviceRemote.IsOnline) throw new InvalidOperationException("Moving playback needs a connection to the server: this device is offline.");
        if (!_serviceRemote.HasSession) throw new InvalidOperationException("There is nothing playing on any device to move.");
        // Moving playback to the device that already plays it is nothing to do; a controller asking for itself is "play here".
        if (deviceID == await _serviceDevice.GetDeviceID() && !IsController) return;

        bool? started = await _serviceMediaHub.Invoke<bool?>(MediaHubMethodEnum.Transfer, deviceID);
        if (started is not true) throw new InvalidOperationException("The server could not start moving playback to that device.");
    }

    public async Task<bool> Continue()
    {
        if (!CanContinue) return false;

        TimeSpan? position = await _serviceQueue.LoadLocalCopy();
        if (position is null || _serviceQueue.Queue.Current is null) return false;

        await _servicePlayer.PlayCurrent();
        if (position > TimeSpan.Zero && _servicePlayer.IsPlaying) await _servicePlayer.Seek(position.Value);

        return true;
    }

    #endregion -- Devices and continuing --

    // A controller sends the command to the active device and changes nothing here. Anything else, and a command nobody is there to receive, runs on this device (which then claims the active role when it plays).
    private async Task Route(RequestPlayerCommand command, Func<Task> local)
    {
        if (!IsController)
        {
            await local();
            return;
        }

        CommandResultEnum? result = await _serviceMediaHub.Invoke<CommandResultEnum?>(MediaHubMethodEnum.SendCommand, command);
        if (result == CommandResultEnum.Sent) return;

        if (result == CommandResultEnum.Invalid)
        {
            _logger.LogWarning("The server found a player command not valid ({Command}).", command.Command);
            return;
        }

        await local();
        if (_servicePlayer.IsPlaying) await _serviceReporter.ClaimNow();
    }
}
