using System.Text.Json;
using Microsoft.Extensions.Logging;
using Noxy.NET.MediaPlayer.Domain.Declarations;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Models;
using Noxy.NET.MediaPlayer.Domain.Responses.Remote;
using Noxy.NET.MediaPlayer.Presentation.Enums;
using Noxy.NET.MediaPlayer.Presentation.Exceptions;
using Noxy.NET.MediaPlayer.Presentation.Extensions;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;
using Noxy.NET.MediaPlayer.Presentation.Models;

namespace Noxy.NET.MediaPlayer.Presentation.Services;

public class PlayerService(
    IAudioPlayer playerAudio,
    IDownloadService serviceDownload,
    IFileStore storeFile,
    IMetadataStore storeMetadata,
    ISyncService serviceSync,
    IVolumeStore storeVolume,
    IQueueService serviceQueue,
    IQueueSourceService serviceQueueSource,
    ISettingService serviceSetting,
    IApiClient clientApi,
    IDelay delay,
    ILogger<PlayerService> logger) : IPlayerService
{
    // Past this much of the current item, Previous starts it again instead of going back.
    private static readonly TimeSpan RestartAfter = TimeSpan.FromSeconds(3);

    private readonly Lock _lock = new();

    // The state below is guarded by _lock; nothing awaits while holding it. Every change to what should play bumps _version, so a play that was overtaken (by a Next, a Clear, another start) notices after each await and gives up.
    private readonly HashSet<Guid> _failedSet = [];
    private readonly Dictionary<Guid, FileSyncStateEnum> _stateMap = [];
    private int _version;
    private bool _wired;
    private bool _waiting;
    private bool _loaded;
    private bool _loadFailed;
    private double _volume = 1;
    private bool _volumeLoaded;
    private bool _positionJumped;
    private int _repeatPassCount;
    private ShuffleModeEnum _shuffle = ShuffleModeEnum.Off;
    private RepeatModeEnum _repeat = RepeatModeEnum.No;

    // A mode the user changed that the server has not been told of yet: what the screen shows is then the user's, not the stored one. Guarded by _lock.
    private readonly PendingMode _shufflePending = new();
    private readonly PendingMode _repeatPending = new();

    private Action? _changed;

    // Subscribing wires the player to the queue, the downloads and the settings, so a device that only watches (a controller) still hears of changes before it ever plays.
    public event Action? Changed
    {
        add
        {
            _changed += value;
            EnsureWired();
        }
        remove => _changed -= value;
    }

    public QueueItem? Current => serviceQueue.Queue.Current;

    public LiveQueue Queue => serviceQueue.Queue;

    public bool IsPlaying => playerAudio.IsPlaying;

    public bool IsDisabled => Current is null;

    public TimeSpan Position => playerAudio.Position;

    public TimeSpan Duration => playerAudio.Duration;

    public double Volume
    {
        get
        {
            lock (_lock) return _volume;
        }
    }

    public bool IsWaiting
    {
        get
        {
            lock (_lock) return _waiting;
        }
    }

    public ShuffleModeEnum Shuffle
    {
        get
        {
            lock (_lock) return _shuffle;
        }
    }

    public RepeatModeEnum Repeat
    {
        get
        {
            lock (_lock) return _repeat;
        }
    }

    public Guid? PlaylistID => serviceQueue.Queue.Source?.PlaylistID;

    public bool PositionJumped
    {
        get
        {
            lock (_lock) return _positionJumped;
        }
        set
        {
            lock (_lock) _positionJumped = value;
        }
    }

    public FileSyncStateEnum StateOf(QueueItem item)
    {
        lock (_lock) return _failedSet.Contains(item.ItemID) ? FileSyncStateEnum.Failed : _stateMap.GetValueOrDefault(item.MediaFileID, FileSyncStateEnum.NotDownloaded);
    }

    #region -- Starting --

    public async Task StartLibrary(Guid mediaFileID, LibrarySortEnum sort, bool descending, string search)
    {
        await StartSource(new() { Kind = QueueSourceKindEnum.Library, PlaylistID = null, Sort = sort, Descending = descending, Search = search }, mediaFileID);
    }

    public async Task StartPlaylist(Guid playlistID, Guid? startEntryID = null)
    {
        if (await StartSource(new() { Kind = QueueSourceKindEnum.Playlist, PlaylistID = playlistID, Sort = LibrarySortEnum.Title, Descending = false, Search = string.Empty }, startEntryID)) await ReportPlaylistPlayed(playlistID);
    }

    public async Task PlayCurrent()
    {
        EnsureWired();
        if (Current is null) return;

        int version;
        lock (_lock) version = BeginNewPlay();

        await RefreshModes();
        await RefreshStates();
        await StartCurrent(version);
    }

    private async Task<bool> StartSource(ResponseQueueSource source, Guid? startID)
    {
        EnsureWired();

        ShuffleModeEnum shuffle = await serviceSetting.GetShuffle();
        if (!await serviceQueue.StartSource(source, startID, shuffle)) return false;

        int version;
        lock (_lock)
        {
            _failedSet.Clear();
            _repeatPassCount = 0;
            version = BeginNewPlay();
        }

        await RefreshModes();
        await RefreshStates();
        await StartCurrent(version);

        return true;
    }

    // The playlist's "last played" is the server's to keep; a device that cannot reach it just plays on.
    private async Task ReportPlaylistPlayed(Guid playlistID)
    {
        try
        {
            await clientApi.MarkPlaylistPlayed(playlistID);
        }
        catch (Exception exception) when (exception.IsOfflineFailure || exception is ApiException)
        {
            logger.LogWarning("A playlist could not be reported as played ({ExceptionType}).", exception.GetType().Name);
        }
    }

    #endregion -- Starting --

    #region -- Controls --

    public async Task Pause()
    {
        if (!IsLoaded) return;

        await playerAudio.Pause();
        _changed?.Invoke();
    }

    public async Task<bool> Resume()
    {
        if (!IsLoaded) return false;

        await playerAudio.Play();
        _changed?.Invoke();

        return true;
    }

    public async Task Seek(TimeSpan position)
    {
        if (!IsLoaded) return;

        await playerAudio.Seek(position);
        lock (_lock) _positionJumped = true;
        _changed?.Invoke();
    }

    public async Task Next()
    {
        EnsureWired();
        if (Current is null) return;

        QueueItem? next = await FindNext();
        if (next is null) return;

        int version;
        lock (_lock) version = BeginNewPlay();

        await serviceQueue.Apply(new() { Edit = QueueEditEnum.Advance, Item = next });
        await StartCurrent(version);
    }

    public async Task JumpTo(Guid itemID, bool keepSkipped)
    {
        EnsureWired();

        ResponseQueue queue = serviceQueue.GetSnapshot();
        QueueItem? ahead = queue.UserQueueList.Concat(queue.SourceList).FirstOrDefault(x => x.ItemID == itemID);
        QueueItem? behind = queue.HistoryList.FirstOrDefault(x => x.ItemID == itemID);
        QueueEdit? edit = ahead is not null
            ? new() { Edit = QueueEditEnum.Advance, Item = ahead, KeepSkipped = keepSkipped }
            : behind is not null
                ? new() { Edit = QueueEditEnum.Previous, Item = behind, KeepSkipped = keepSkipped }
                : null;
        if (edit is null) return;

        int version;
        lock (_lock) version = BeginNewPlay();

        await serviceQueue.Apply(edit);
        await StartCurrent(version);
    }

    public async Task Previous()
    {
        EnsureWired();
        if (Current is not { } current) return;

        // A media file that was deleted while it plays only ever restarts: going back would put it into the queue again.
        bool deleted = await storeMetadata.GetMediaFile(current.MediaFileID) is null;
        if (!deleted && Position <= RestartAfter && LastPlayed() is { } previous)
        {
            int version;
            lock (_lock) version = BeginNewPlay();

            await serviceQueue.Apply(new() { Edit = QueueEditEnum.Previous, Item = previous });
            await StartCurrent(version);
            return;
        }

        if (!IsLoaded) return;

        await playerAudio.Seek(TimeSpan.Zero);
        await playerAudio.Play();
        lock (_lock) _positionJumped = true;
        _changed?.Invoke();
    }

    // The last item of the history that did not fail; null when there is none.
    private QueueItem? LastPlayed()
    {
        List<QueueItem> historyList = serviceQueue.GetSnapshot().HistoryList;
        lock (_lock) return historyList.LastOrDefault(x => !_failedSet.Contains(x.ItemID));
    }

    // The mode changes on screen and in the queue at once; the server hears of it after a pause, and only of the last change.
    public async Task SetShuffle(ShuffleModeEnum shuffle)
    {
        EnsureWired();

        // The mode and the pending mark change together, or a refresh of the stored modes could slip in between and take the change back.
        lock (_lock)
        {
            _shuffle = shuffle;
            _shufflePending.IsPending = true;
        }

        _changed?.Invoke();
        SaveLater(_shufflePending, SettingKey.Shuffle, shuffle.ToString());

        await serviceQueue.Regenerate(shuffle);
    }

    public Task SetRepeat(RepeatModeEnum repeat)
    {
        EnsureWired();

        lock (_lock)
        {
            _repeat = repeat;
            _repeatPending.IsPending = true;
        }

        _changed?.Invoke();
        SaveLater(_repeatPending, SettingKey.Repeat, repeat.ToString());

        return Task.CompletedTask;
    }

    // A later change of the same mode takes over: the earlier wait ends and sends nothing. When the newest one has been answered, the screen follows the stored value again, which also takes back a change the server refused.
    private void SaveLater(PendingMode mode, string key, string value)
    {
        CancellationTokenSource source = new();
        CancellationTokenSource? previous;
        lock (_lock)
        {
            previous = mode.Source;
            mode.Source = source;
            mode.IsPending = true;
        }

        previous?.Cancel();

        _ = Guard(async () =>
        {
            try
            {
                await delay.Wait(TimeSpan.FromMilliseconds(PresentationConstants.ModeSaveMilliseconds), source.Token);
                await serviceSetting.Update(key, JsonSerializer.SerializeToElement(value));
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception exception)
            {
                // The type only: the message is not ours to log blindly.
                logger.LogWarning("A player mode could not be saved on the server ({ExceptionType}).", exception.GetType().Name);
            }

            lock (_lock)
            {
                if (!ReferenceEquals(mode.Source, source)) return;

                mode.IsPending = false;
            }

            await RefreshModes();
        });
    }

    public async Task Clear()
    {
        EnsureWired();

        lock (_lock)
        {
            _failedSet.Clear();
            BeginNewPlay();
        }

        await serviceQueue.Clear();
        await StopPlayback();
    }

    #endregion -- Controls --

    #region -- Volume --

    // The volume belongs to the device: applied at once and kept in the device's own store, so it survives a restart. A save that fails does not undo the volume.
    public async Task SetVolume(double volume)
    {
        double clamped = await ApplyVolume(volume);

        try
        {
            await storeVolume.SetVolume((int)Math.Round(clamped * 100));
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            // The subject only: the value is not ours to log.
            logger.LogWarning(exception, "The volume could not be saved on the device.");
        }
    }

    // A step of a drag changes the sound at once and writes nothing: the file would be rewritten for every step. The user has still set a volume on this device, so a stored one does not replace it.
    public async Task PreviewVolume(double volume)
    {
        await ApplyVolume(volume);
    }

    private async Task<double> ApplyVolume(double volume)
    {
        double clamped = Math.Clamp(volume, 0, 1);
        lock (_lock)
        {
            _volume = clamped;
            _volumeLoaded = true;
        }

        await playerAudio.SetVolume(clamped);
        _changed?.Invoke();

        return clamped;
    }

    // Reads the stored volume once, so Volume is right before anything has played. A volume the user already set on this device wins.
    public async Task LoadVolume()
    {
        await ResolveVolume();
        _changed?.Invoke();
    }

    // The volume is cosmetic: a stored value that cannot be read must never stop a media file from playing.
    private async Task<double> ResolveVolume()
    {
        lock (_lock)
        {
            if (_volumeLoaded) return _volume;
        }

        int stored;
        try
        {
            stored = await storeVolume.GetVolume();
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogWarning(exception, "The stored volume could not be read.");
            lock (_lock) return _volume;
        }

        lock (_lock)
        {
            if (_volumeLoaded) return _volume;

            _volume = Math.Clamp(stored, 0, 100) / 100.0;
            _volumeLoaded = true;

            return _volume;
        }
    }

    #endregion -- Volume --

    #region -- Playing the current item --

    private bool IsLoaded
    {
        get
        {
            lock (_lock) return _loaded;
        }
    }

    // Under _lock. Whatever was starting is overtaken.
    private int BeginNewPlay()
    {
        _waiting = false;

        return ++_version;
    }

    // The next item that can play: the user queue first, then the source part; a failed item is stepped over. When the queue ran out the repeat mode may append the source again. Null when nothing is left.
    private async Task<QueueItem?> FindNext()
    {
        return FirstPlayable() ?? await AppendRepeat();
    }

    private QueueItem? FirstPlayable()
    {
        ResponseQueue queue = serviceQueue.GetSnapshot();
        lock (_lock) return queue.UserQueueList.Concat(queue.SourceList).FirstOrDefault(x => !_failedSet.Contains(x.ItemID));
    }

    // Repeat Once appends the source once more, Forever each time it ends; the pass is built again, so a shuffle comes out in a new order. A source that has been emptied ends the queue instead.
    private async Task<QueueItem?> AppendRepeat()
    {
        ResponseQueueSource? source = serviceQueue.Queue.Source;
        if (source is null) return null;

        RepeatModeEnum repeat = await serviceSetting.GetRepeat();
        lock (_lock)
        {
            if (repeat == RepeatModeEnum.No || (repeat == RepeatModeEnum.Once && _repeatPassCount >= 1)) return null;
        }

        List<QueueItem>? pass = await serviceQueueSource.Build(source, await serviceSetting.GetShuffle(), null);
        if (pass is null || pass.Count == 0) return null;

        // A playlist entry is already in the queue's history under its own ID, and an ID may appear only once: each repeated item is new.
        List<QueueItem> fresh = pass.Select(x => new QueueItem(Guid.NewGuid(), x.MediaFileID)).ToList();
        await serviceQueue.Apply(new() { Edit = QueueEditEnum.RegenerateSource, ItemList = [.. fresh] });
        lock (_lock) _repeatPassCount++;

        return fresh[0];
    }

    // Plays the queue's current item (or only loads it, paused, when play is false): stops what was playing (so its file is released), gets the file onto the device if it is missing, loads it and plays. An item that cannot play is marked failed and the next one is tried.
    private async Task StartCurrent(int version, bool play = true)
    {
        while (true)
        {
            QueueItem? item = serviceQueue.Queue.Current;
            lock (_lock)
            {
                if (version != _version) return;
            }

            if (item is null)
            {
                await StopPlayback();
                return;
            }

            await playerAudio.Stop();
            lock (_lock) _loaded = false;
            _changed?.Invoke();

            string? path = null;
            if (await storeMetadata.GetMediaFile(item.MediaFileID) is not null)
            {
                path = await storeFile.GetPath(item.MediaFileID);
                if (path is null) path = await Download(item.MediaFileID, version);
                if (version != _version) return;
            }

            if (path is null)
            {
                QueueItem? next;
                lock (_lock) _failedSet.Add(item.ItemID);

                next = FirstPlayable();
                if (next is null)
                {
                    _changed?.Invoke();
                    return;
                }

                await serviceQueue.Apply(new() { Edit = QueueEditEnum.Advance, Item = next });
                continue;
            }

            lock (_lock) _loadFailed = false;
            await playerAudio.Load(path);

            // A load that failed is moved on from by the failure handler; an overtaken play must not start the file.
            lock (_lock)
            {
                if (_loadFailed || version != _version) return;

                _loaded = true;
            }

            await playerAudio.SetVolume(await ResolveVolume());
            if (play) await playerAudio.Play();
            _changed?.Invoke();

            await Prefetch();
            return;
        }
    }

    private async Task<string?> Download(Guid mediaFileID, int version)
    {
        lock (_lock) _waiting = true;
        _changed?.Invoke();

        bool onDevice = await serviceDownload.EnsureOnDevice(mediaFileID);

        lock (_lock)
        {
            if (version == _version) _waiting = false;
        }

        _changed?.Invoke();

        return onDevice && version == _version ? await storeFile.GetPath(mediaFileID) : null;
    }

    // The files of the next items (as many as the PrefetchCount setting says) that are not on the device yet are asked for. Nothing beyond that window is started.
    private async Task Prefetch()
    {
        int count = await serviceSetting.GetPrefetchCount();
        if (count <= 0) return;

        ResponseQueue queue = serviceQueue.GetSnapshot();
        List<Guid> windowList;
        lock (_lock) windowList = queue.UserQueueList.Concat(queue.SourceList).Where(x => !_failedSet.Contains(x.ItemID)).Select(x => x.MediaFileID).Distinct().Take(count).ToList();

        List<Guid> missingList = [];
        foreach (Guid id in windowList)
        {
            if (await storeMetadata.GetMediaFile(id) is not null && await storeFile.GetPath(id) is null) missingList.Add(id);
        }

        if (missingList.Count > 0) await serviceDownload.Enqueue(missingList);
    }

    private async Task StopPlayback()
    {
        await playerAudio.Stop();
        lock (_lock) _loaded = false;
        _changed?.Invoke();
    }

    private async Task RefreshStates()
    {
        List<FileState> stateList = await storeMetadata.GetFileStateList();
        lock (_lock)
        {
            _stateMap.Clear();
            foreach (FileState state in stateList) _stateMap[state.MediaFileID] = state.State;
        }

        _changed?.Invoke();
    }

    private async Task RefreshModes()
    {
        ShuffleModeEnum shuffle = await serviceSetting.GetShuffle();
        RepeatModeEnum repeat = await serviceSetting.GetRepeat();

        bool changed = false;
        lock (_lock)
        {
            // A mode the server has not heard of yet is the user's and stays as it is.
            if (!_shufflePending.IsPending)
            {
                changed |= shuffle != _shuffle;
                _shuffle = shuffle;
            }

            if (!_repeatPending.IsPending)
            {
                changed |= repeat != _repeat;
                _repeat = repeat;
            }
        }

        if (changed) _changed?.Invoke();
    }

    #endregion -- Playing the current item --

    #region -- Events --

    // The audio player says the file played to its end: the next playable item starts, or at the end of the queue the last one goes to history and everything stops and the file is released.
    private async Task OnEnded()
    {
        lock (_lock)
        {
            if (!_loaded) return;
        }

        QueueItem? next = await FindNext();

        int version;
        lock (_lock)
        {
            if (!_loaded) return;

            version = BeginNewPlay();
        }

        if (next is null)
        {
            await serviceQueue.Apply(new() { Edit = QueueEditEnum.Advance });
            await StopPlayback();
            return;
        }

        await serviceQueue.Apply(new() { Edit = QueueEditEnum.Advance, Item = next });
        await StartCurrent(version);
    }

    // The audio player could not decode the file: the item is marked failed (its verified file stays) and the next one plays.
    private async Task OnFailed()
    {
        if (serviceQueue.Queue.Current is not { } item) return;

        int version;
        lock (_lock)
        {
            _failedSet.Add(item.ItemID);
            version = BeginNewPlay();
        }

        QueueItem? next = FirstPlayable();
        if (next is null)
        {
            await StopPlayback();
            return;
        }

        await serviceQueue.Apply(new() { Edit = QueueEditEnum.Advance, Item = next });
        await StartCurrent(version);
    }

    // The queue removed the item that was playing (a controller removed it): skip at once. It never enters history. What comes next is loaded and plays only if something was playing; a paused player stays paused.
    private async Task OnCurrentRemoved()
    {
        bool wasPlaying = playerAudio.IsPlaying;
        int version;
        lock (_lock) version = BeginNewPlay();

        QueueItem? next = await FindNext();
        if (next is null)
        {
            await StopPlayback();
            return;
        }

        await serviceQueue.Apply(new() { Edit = QueueEditEnum.Advance, Item = next });
        await StartCurrent(version, wasPlaying);
    }

    private void EnsureWired()
    {
        lock (_lock)
        {
            if (_wired) return;

            _wired = true;
        }

        playerAudio.Ended += () => Fire(OnEnded);
        playerAudio.Failed += _ =>
        {
            // Not the reason text: it is the platform's and not ours to log blindly. The synchronous part tells a load in progress that it failed.
            lock (_lock) _loadFailed = true;
            logger.LogWarning("The audio player could not play a file.");
            Fire(OnFailed);
        };
        playerAudio.Changed += () => _changed?.Invoke();
        serviceDownload.Changed += () => Fire(RefreshStates);
        serviceSync.LibraryDiffApplied += _ => Fire(RefreshStates);
        serviceSync.Changed += () => Fire(RefreshModes);
        serviceSetting.Changed += () => Fire(RefreshModes);
        serviceQueue.Changed += () =>
        {
            _changed?.Invoke();
            Fire(RefreshModes);
        };
        serviceQueue.CurrentRemoved += () => Fire(OnCurrentRemoved);
    }

    private void Fire(Func<Task> work)
    {
        _ = Guard(work);
    }

    private async Task Guard(Func<Task> work)
    {
        try
        {
            await Task.Run(work);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "The player ended a background step with an error.");
        }
    }

    #endregion -- Events --
}
