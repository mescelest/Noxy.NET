using System.Text.Json;
using Microsoft.Extensions.Logging;
using Noxy.NET.MediaPlayer.Domain.Declarations;
using Noxy.NET.MediaPlayer.Presentation.Enums;
using Noxy.NET.MediaPlayer.Presentation.Exceptions;
using Noxy.NET.MediaPlayer.Presentation.Extensions;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;
using Noxy.NET.MediaPlayer.Presentation.Models;

namespace Noxy.NET.MediaPlayer.Presentation.Services;

public class PlayerService(IAudioPlayer playerAudio, IDownloadService serviceDownload, IFileStore storeFile, IMetadataStore storeMetadata, ISyncService serviceSync, ISettingService serviceSetting, ILogger<PlayerService> logger) : IPlayerService
{
    private readonly Lock _lock = new();

    // The state below is guarded by _lock; nothing awaits while holding it. Every change to what should play bumps _version, so a play that was overtaken (by a Next, a Clear, another Play) notices after each await and gives up.
    private readonly List<(Guid ID, bool Failed)> _queue = [];
    private readonly HashSet<Guid> _goneSet = [];
    private readonly Dictionary<Guid, FileSyncStateEnum> _stateMap = [];
    private int _index = -1;
    private int _version;
    private bool _wired;
    private bool _waiting;
    private bool _loaded;
    private bool _loadFailed;
    private double _volume = 1;
    private bool _volumeSynced;
    private bool _writing;
    private int? _pendingWrite;

    public event Action? Changed;

    public PlayerQueueEntry? Current
    {
        get
        {
            lock (_lock) return EntryAt(_index);
        }
    }

    public List<PlayerQueueEntry> QueueList
    {
        get
        {
            lock (_lock)
            {
                List<PlayerQueueEntry> list = [];
                for (int i = 0; i < _queue.Count; i++)
                {
                    if (EntryAt(i) is { } entry) list.Add(entry);
                }

                return list;
            }
        }
    }

    public bool IsPlaying => playerAudio.IsPlaying;

    public TimeSpan Position => playerAudio.Position;

    public TimeSpan Duration => playerAudio.Duration;

    public double Volume
    {
        get
        {
            lock (_lock) return _volume;
        }
    }

    public bool IsWaitingForDownload
    {
        get
        {
            lock (_lock) return _waiting;
        }
    }

    public async Task Play(Guid mediaFileID)
    {
        await PlayList([mediaFileID], 0);
    }

    public async Task PlayList(List<Guid> mediaFileIDList, int startIndex)
    {
        EnsureWired();

        int version;
        int index;
        lock (_lock)
        {
            _queue.Clear();
            _queue.AddRange(mediaFileIDList.Select(x => (x, false)));
            index = _queue.Count == 0 ? -1 : Math.Clamp(startIndex, 0, _queue.Count - 1);
            _index = index;
            version = BeginNewPlay();
        }

        if (index < 0)
        {
            await StopPlayback();
            return;
        }

        await RefreshStates();
        await StartAt(index, version);
    }

    public async Task Enqueue(Guid mediaFileID)
    {
        EnsureWired();

        lock (_lock) _queue.Add((mediaFileID, false));

        Changed?.Invoke();
        await Prefetch();
    }

    public async Task Remove(int index)
    {
        EnsureWired();

        bool wasCurrent;
        int next = -1;
        int version = 0;
        lock (_lock)
        {
            if (index < 0 || index >= _queue.Count) return;

            wasCurrent = index == _index;
            _queue.RemoveAt(index);
            if (index < _index) _index--;

            if (wasCurrent)
            {
                version = BeginNewPlay();
                _index = index - 1;
                next = FirstPlayableFrom(index);
                if (next < 0) _index = _queue.Count - 1;
            }
        }

        Changed?.Invoke();

        if (!wasCurrent)
        {
            await Prefetch();
            return;
        }

        if (next >= 0) await StartAt(next, version);
        else await StopPlayback();
    }

    public async Task Pause()
    {
        if (!IsLoaded) return;

        await playerAudio.Pause();
        Changed?.Invoke();
    }

    public async Task Resume()
    {
        if (!IsLoaded) return;

        await playerAudio.Play();
        Changed?.Invoke();
    }

    public async Task Seek(TimeSpan position)
    {
        if (!IsLoaded) return;

        await playerAudio.Seek(position);
        Changed?.Invoke();
    }

    public async Task Next()
    {
        EnsureWired();

        int next;
        int version;
        lock (_lock)
        {
            if (_index < 0) return;

            next = FirstPlayableFrom(_index + 1);
            if (next < 0) return;

            version = BeginNewPlay();
        }

        await StartAt(next, version);
    }

    public async Task Previous()
    {
        EnsureWired();

        int previous = -1;
        int version = 0;
        lock (_lock)
        {
            if (_index < 0) return;

            for (int i = _index - 1; i >= 0; i--)
            {
                if (_queue[i].Failed) continue;

                previous = i;
                break;
            }

            if (previous >= 0) version = BeginNewPlay();
        }

        if (previous >= 0)
        {
            await StartAt(previous, version);
            return;
        }

        // At the start of the queue: the current media file starts again.
        if (!IsLoaded) return;

        await playerAudio.Seek(TimeSpan.Zero);
        await playerAudio.Play();
        Changed?.Invoke();
    }

    // Applies at once, then writes the setting so the other devices follow. A write that fails does not undo the local volume. While a write is in flight a setting change is not applied, so a slider drag is not pulled back by the answer to an earlier write.
    public async Task SetVolume(double volume)
    {
        double clamped = Math.Clamp(volume, 0, 1);
        bool startWriter;
        lock (_lock)
        {
            _volume = clamped;
            _volumeSynced = true;
            _pendingWrite = (int)Math.Round(clamped * 100);
            startWriter = !_writing;
            _writing = true;
        }

        await playerAudio.SetVolume(clamped);
        Changed?.Invoke();

        // One writer at a time keeps the writes in order. A change made while one is under way only replaces the value waiting to be written, so the last value is always the last write.
        if (startWriter) await WriteVolumes();
    }

    private async Task WriteVolumes()
    {
        while (true)
        {
            int value;
            lock (_lock)
            {
                if (_pendingWrite is not { } pending)
                {
                    _writing = false;
                    return;
                }

                value = pending;
                _pendingWrite = null;
            }

            try
            {
                await serviceSetting.Update(SettingKey.Volume, JsonSerializer.SerializeToElement(value));
            }
            catch (Exception exception) when (exception.IsOfflineFailure || exception is ApiException)
            {
                // The subject only: the value is not ours to log.
                logger.LogWarning("The volume could not be saved to the server.");
            }
            catch
            {
                lock (_lock) _writing = false;
                throw;
            }
        }
    }

    // The stored volume is applied the first time a media file starts, unless the user already set one on this device.
    private async Task<double> ResolveVolume()
    {
        lock (_lock)
        {
            if (_volumeSynced) return _volume;
        }

        // The volume is cosmetic: a stored value that cannot be read must never stop a media file from playing.
        int stored;
        try
        {
            stored = await serviceSetting.GetVolume();
        }
        catch (InvalidOperationException)
        {
            logger.LogWarning("The stored volume could not be read.");
            lock (_lock) return _volume;
        }

        lock (_lock)
        {
            if (_volumeSynced) return _volume;

            _volume = stored / 100.0;
            _volumeSynced = true;

            return _volume;
        }
    }

    // Another device changed the volume. A volume applied for that reason is never written back.
    private async Task OnSettingChanged()
    {
        int stored = await serviceSetting.GetVolume();
        double volume;
        lock (_lock)
        {
            if (_writing || stored == (int)Math.Round(_volume * 100)) return;

            _volume = stored / 100.0;
            _volumeSynced = true;
            volume = _volume;
        }

        await playerAudio.SetVolume(volume);
        Changed?.Invoke();
    }

    public async Task Clear()
    {
        EnsureWired();

        lock (_lock)
        {
            _queue.Clear();
            _index = -1;
            BeginNewPlay();
        }

        await StopPlayback();
    }

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

    // Under _lock. A queue entry's state is what its file is like, or Failed when it could not be played.
    private PlayerQueueEntry? EntryAt(int index)
    {
        if (index < 0 || index >= _queue.Count) return null;

        (Guid id, bool failed) = _queue[index];

        return new(id, failed ? FileSyncStateEnum.Failed : _stateMap.GetValueOrDefault(id, FileSyncStateEnum.NotDownloaded));
    }

    // Under _lock. Entries for deleted media files are dropped on the way; failed entries are stepped over. -1 when nothing after (and including) start can play.
    private int FirstPlayableFrom(int start)
    {
        for (int i = _queue.Count - 1; i >= start && i >= 0; i--)
        {
            if (_goneSet.Contains(_queue[i].ID) && i != _index) _queue.RemoveAt(i);
        }

        for (int i = Math.Max(start, 0); i < _queue.Count; i++)
        {
            if (!_queue[i].Failed) return i;
        }

        return -1;
    }

    // Plays the entry at the index: stops what was playing (so its file is released), gets the file onto the device if it is missing, loads it and plays. An entry that cannot play is marked failed and the next one is tried.
    private async Task StartAt(int index, int version)
    {
        while (true)
        {
            Guid id;
            lock (_lock)
            {
                if (version != _version || index < 0 || index >= _queue.Count) return;

                _index = index;
                id = _queue[index].ID;
            }

            await playerAudio.Stop();
            lock (_lock) _loaded = false;
            Changed?.Invoke();

            string? path = null;
            bool exists = !IsGone(id) && await storeMetadata.GetMediaFile(id) is not null;
            if (exists)
            {
                path = await storeFile.GetPath(id);
                if (path is null) path = await Download(id, version);
                if (version != _version) return;
            }

            if (path is null)
            {
                index = FailAndFindNext(index);
                if (index < 0)
                {
                    Changed?.Invoke();
                    return;
                }

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
            await playerAudio.Play();
            Changed?.Invoke();

            await Prefetch();
            return;
        }
    }

    private async Task<string?> Download(Guid id, int version)
    {
        lock (_lock) _waiting = true;
        Changed?.Invoke();

        bool onDevice = await serviceDownload.EnsureOnDevice(id);

        lock (_lock)
        {
            if (version == _version) _waiting = false;
        }

        Changed?.Invoke();

        return onDevice && version == _version ? await storeFile.GetPath(id) : null;
    }

    private bool IsGone(Guid id)
    {
        lock (_lock) return _goneSet.Contains(id);
    }

    // The entry could not be played: mark it, and find the next one that can. -1 when there is none.
    private int FailAndFindNext(int index)
    {
        lock (_lock)
        {
            if (index >= 0 && index < _queue.Count) _queue[index] = (_queue[index].ID, true);

            return FirstPlayableFrom(index + 1);
        }
    }

    // The files of the next PrefetchCount queue entries that are not on the device yet are asked for. Nothing beyond that window is started.
    private async Task Prefetch()
    {
        List<Guid> windowList;
        lock (_lock)
        {
            windowList = _queue.Skip(_index + 1).Take(Constants.PrefetchCount).Where(x => !x.Failed && !_goneSet.Contains(x.ID)).Select(x => x.ID).Distinct().ToList();
        }

        List<Guid> missingList = [];
        foreach (Guid id in windowList)
        {
            if (await storeFile.GetPath(id) is null) missingList.Add(id);
        }

        if (missingList.Count > 0) await serviceDownload.Enqueue(missingList);
    }

    private async Task StopPlayback()
    {
        await playerAudio.Stop();
        lock (_lock) _loaded = false;
        Changed?.Invoke();
    }

    private async Task RefreshStates()
    {
        List<FileState> stateList = await storeMetadata.GetFileStateList();
        lock (_lock)
        {
            _stateMap.Clear();
            foreach (FileState state in stateList) _stateMap[state.MediaFileID] = state.State;
        }

        Changed?.Invoke();
    }

    // The audio player says the file played to its end: the next playable entry starts, or at the end of the queue everything stops and the file is released.
    private async Task OnEnded()
    {
        int next;
        int version;
        lock (_lock)
        {
            if (!_loaded) return;

            next = FirstPlayableFrom(_index + 1);
            version = BeginNewPlay();
        }

        if (next >= 0) await StartAt(next, version);
        else await StopPlayback();
    }

    // The audio player could not decode the file: the entry is marked failed (its verified file stays) and the next one plays.
    private async Task OnFailed()
    {
        int next;
        int version;
        lock (_lock)
        {
            if (_index < 0) return;

            next = FailAndFindNext(_index);
            version = BeginNewPlay();
        }

        if (next >= 0) await StartAt(next, version);
        else await StopPlayback();
    }

    private void OnLibraryDiff(LibraryDiff diff)
    {
        lock (_lock)
        {
            foreach (Guid id in diff.RemovedIDList)
            {
                _goneSet.Add(id);

                // The one playing finishes first; any other entry for it leaves the queue now.
                for (int i = _queue.Count - 1; i >= 0; i--)
                {
                    if (_queue[i].ID != id || i == _index) continue;

                    _queue.RemoveAt(i);
                    if (i < _index) _index--;
                }
            }
        }

        Changed?.Invoke();
        Fire(RefreshStates);
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
        playerAudio.Changed += () => Changed?.Invoke();
        serviceDownload.Changed += () => Fire(RefreshStates);
        serviceSync.LibraryDiffApplied += OnLibraryDiff;
        serviceSetting.Changed += () => Fire(OnSettingChanged);
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
}
