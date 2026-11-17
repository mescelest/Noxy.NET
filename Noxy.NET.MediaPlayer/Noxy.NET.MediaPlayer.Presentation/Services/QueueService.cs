using Microsoft.Extensions.Logging;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Models;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;
using Noxy.NET.MediaPlayer.Domain.Responses.Remote;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Services;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;
using Noxy.NET.MediaPlayer.Presentation.Models;

namespace Noxy.NET.MediaPlayer.Presentation.Services;

public class QueueService : IQueueService
{
    private sealed class Outcome
    {
        public List<QueueEdit> AppliedList { get; } = [];
        public bool Changed { get; set; }
        public bool CurrentRemoved { get; set; }
        public bool Gap { get; set; }
    }

    private readonly SemaphoreSlim _lock = new(1, 1);

    // Guards the queue object itself: what reads it from outside (GetSnapshot) never sees it half-changed.
    private readonly Lock _stateLock = new();
    private readonly IQueueSourceService _serviceSource;
    private readonly ISettingService _serviceSetting;
    private readonly ILocalQueueStore _storeLocalQueue;
    private readonly IMetadataStore _storeMetadata;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<QueueService> _logger;
    private LiveQueue _queue = new();
    private double _positionSeconds;

    // A playing item that was deleted from the library or its playlist finishes first: it stays current until the queue moves on, then it is dropped and never enters history.
    private Guid? _ghostItemID;

    // The entries of the playing playlist as the queue last knew them, so that only a really new entry joins the queue and one the user removed is not put back. Null until it is read.
    private HashSet<Guid>? _knownEntryIDSet;

    public QueueService(IQueueSourceService serviceSource, ISettingService serviceSetting, ILocalQueueStore storeLocalQueue, ISyncService serviceSync, IMetadataStore storeMetadata, TimeProvider timeProvider, ILogger<QueueService> logger)
    {
        _serviceSource = serviceSource;
        _serviceSetting = serviceSetting;
        _storeLocalQueue = storeLocalQueue;
        _storeMetadata = storeMetadata;
        _timeProvider = timeProvider;
        _logger = logger;

        serviceSync.LibraryDiffApplied += diff => _ = RunSourceChange(() => HandleLibraryDiff(diff));
        serviceSync.Changed += () => _ = RunSourceChange(HandlePlaylistChange);
    }

    public event Action? Changed;

    public event Action<QueueEdit>? EditApplied;

    public event Action? GapDetected;

    public event Action? CurrentRemoved;

    public LiveQueue Queue => _queue;

    public bool OwnsPlayback { get; set; }

    public ResponseQueue GetSnapshot()
    {
        lock (_stateLock) return _queue.ToResponse();
    }

    #region -- Operations --

    public async Task<bool> Apply(QueueEdit edit)
    {
        Outcome done = await Locked(async outcome => { ApplyLocked(edit, outcome); });

        Publish(done);
        return done.AppliedList.Count > 0;
    }

    public async Task ApplyDelta(ResponseQueueDelta delta)
    {
        Outcome done = await Locked(async outcome =>
        {
            if (delta.QueueRevision <= _queue.QueueRevision) return;
            if (delta.QueueRevision > _queue.QueueRevision + 1)
            {
                outcome.Gap = true;
                return;
            }

            QueueEdit edit = ToEdit(delta);
            QueueItem? before = _queue.Current;
            try
            {
                lock (_stateLock) _queue.Apply(edit, Constants.QueueHistoryLimit);
            }
            catch (InvalidQueueEditException)
            {
                outcome.Gap = true;
                return;
            }

            // The server's number wins, also when the edit changed nothing here.
            lock (_stateLock) _queue.QueueRevision = delta.QueueRevision;
            outcome.Changed = true;
            AfterChange(before, edit, outcome);
        });

        Publish(done);
    }

    public async Task ReplaceWithSnapshot(ResponseQueue snapshot, bool keepLocalPointer)
    {
        Outcome done = await Locked(async outcome =>
        {
            ResponseQueue local = _queue.ToResponse();
            ResponseQueue target = snapshot;
            if (keepLocalPointer && local.Current is { } current)
            {
                // A snapshot with no current item is what a Remove of the playing item leaves. One that merely lacks this device's current item is older than the edits this device made since (the queue it was made from came before them): the pointer stays.
                if (Contains(snapshot, current.ItemID) || snapshot.Current is not null) target = local.QueueRevision >= snapshot.QueueRevision ? WithRevision(local, snapshot.QueueRevision) : KeepLocalPointer(local, current, snapshot);
                else outcome.CurrentRemoved = OwnsPlayback;
            }

            LiveQueue replaced = LiveQueue.From(target, Constants.QueueHistoryLimit);
            lock (_stateLock) _queue = replaced;
            _ghostItemID = null;
            _knownEntryIDSet = null;
            if (local.Current?.ItemID != target.Current?.ItemID) _positionSeconds = 0;

            outcome.Changed = true;
        });

        Publish(done);
    }

    public async Task<bool> StartSource(ResponseQueueSource source, Guid? startID, ShuffleModeEnum shuffle)
    {
        List<QueueItem>? itemList = await _serviceSource.Build(source, shuffle, startID);
        if (itemList is null || itemList.Count == 0) return false;

        bool keepUserQueue = await _serviceSetting.GetKeepQueuedItems();
        List<QueueItem>? baseList = source.Kind == QueueSourceKindEnum.Playlist ? await _serviceSource.BuildBase(source) : null;
        QueueEdit edit = new() { Edit = QueueEditEnum.ReplaceSource, Source = source, ItemList = [.. itemList], CurrentItemID = itemList[0].ItemID, KeepUserQueue = keepUserQueue };

        Outcome done = await Locked(async outcome =>
        {
            _ghostItemID = null;
            ApplyLocked(edit, outcome);
            _knownEntryIDSet = baseList?.Select(x => x.ItemID).ToHashSet();
        });

        Publish(done);
        return done.AppliedList.Count > 0;
    }

    public async Task Regenerate(ShuffleModeEnum shuffle)
    {
        Outcome done = await Locked(async outcome =>
        {
            ResponseQueue queue = _queue.ToResponse();
            if (queue.Source is null || queue.Current is null) return;

            List<QueueItem>? itemList = await _serviceSource.Regenerate(queue.Source, shuffle, queue.Current, [.. queue.HistoryList, .. queue.UserQueueList]);
            if (itemList is null) return;

            ApplyLocked(new() { Edit = QueueEditEnum.RegenerateSource, ItemList = [.. itemList] }, outcome);
        });

        Publish(done);
    }

    public async Task<TimeSpan?> LoadLocalCopy()
    {
        LocalQueueSnapshot? snapshot = await _storeLocalQueue.Get();
        if (snapshot is null) return null;

        TimeSpan? position = null;
        Outcome done = await Locked(async outcome =>
        {
            try
            {
                LiveQueue loaded = LiveQueue.From(snapshot.Queue, Constants.QueueHistoryLimit);
                lock (_stateLock) _queue = loaded;
            }
            catch (InvalidQueueEditException)
            {
                // A copy that is not a valid queue is of no use: dropped, as if nothing was saved.
                await _storeLocalQueue.Clear();
                return;
            }

            _ghostItemID = null;
            _knownEntryIDSet = null;
            _positionSeconds = snapshot.PositionSeconds;
            position = TimeSpan.FromSeconds(snapshot.PositionSeconds);
            outcome.Changed = true;
        });

        Publish(done);
        return position;
    }

    public async Task SaveLocalCopy(double positionSeconds)
    {
        await Locked(async _ =>
        {
            _positionSeconds = positionSeconds;
            await SaveLocked();
        });
    }

    public async Task Clear()
    {
        Outcome done = await Locked(async outcome =>
        {
            lock (_stateLock) _queue = new();
            _ghostItemID = null;
            _knownEntryIDSet = null;
            _positionSeconds = 0;
            outcome.Changed = true;
            await _storeLocalQueue.Clear();
        });

        Publish(done);
    }

    #endregion -- Operations --

    #region -- The source changes underneath --

    // A change of the library or a playlist reaches the queue only on the device that plays; a failure here never reaches the user, it only leaves the queue as it was.
    private async Task RunSourceChange(Func<Task> change)
    {
        try
        {
            await change();
        }
        catch (Exception exception)
        {
            _logger.LogError("A change of the library or a playlist could not be applied to the queue ({ExceptionType}).", exception.GetType().Name);
        }
    }

    private async Task HandleLibraryDiff(LibraryDiff diff)
    {
        if (!OwnsPlayback) return;

        Outcome done = await Locked(async outcome =>
        {
            if (!OwnsPlayback) return;

            foreach (QueueEdit edit in RemovalEditList(diff.RemovedIDList.ToHashSet(), x => x.MediaFileID)) ApplyLocked(edit, outcome);

            ResponseQueueSource? source = _queue.ToResponse().Source;
            if (source is { Kind: QueueSourceKindEnum.Library })
            {
                ShuffleModeEnum shuffle = await _serviceSource.GetShuffle(source);
                foreach (Guid mediaFileID in diff.AddedList.Select(x => x.ID))
                {
                    QueuePlacement? placement = await _serviceSource.PlaceNew(source, shuffle, _queue, mediaFileID);
                    if (placement is not null) ApplyLocked(new() { Edit = QueueEditEnum.InsertSourceItem, Item = placement.Item, AfterItemID = placement.AfterItemID }, outcome);
                }
            }
        });

        Publish(done);
    }

    // Reads the playing playlist again: a deleted playlist ends the source part, entries that left the playlist leave the queue and new ones join it.
    private async Task HandlePlaylistChange()
    {
        if (!OwnsPlayback) return;

        Outcome done = await Locked(async outcome =>
        {
            ResponseQueue queue = _queue.ToResponse();
            if (!OwnsPlayback || queue.Source is not { Kind: QueueSourceKindEnum.Playlist, PlaylistID: { } playlistID } source) return;

            if (!(await _storeMetadata.GetPlaylistList()).Any(x => x.ID == playlistID))
            {
                _knownEntryIDSet = null;
                if (queue.SourceList.Count > 0) ApplyLocked(new() { Edit = QueueEditEnum.RegenerateSource, ItemList = [] }, outcome);
                return;
            }

            ResponsePlaylistDetail? detail = await _storeMetadata.GetPlaylistDetail(playlistID);
            if (detail is null) return;

            HashSet<Guid> entryIDSet = detail.EntryList.Select(x => x.ID).ToHashSet();
            HashSet<Guid>? known = _knownEntryIDSet;
            _knownEntryIDSet = entryIDSet;
            if (known is null)
            {
                // The first look since the queue was loaded: what the playlist no longer holds leaves the queue. Nothing is added, for it is not known what the user took out of the queue before. Only the source part is looked at: the playing item may be one the user queued, which is no entry of the playlist. A repeated pass has entries of its own IDs, so it is the media file that is looked for.
                HashSet<Guid> mediaFileIDSet = detail.EntryList.Select(x => x.MediaFileID).ToHashSet();
                HashSet<Guid> leftSet = queue.SourceList.Where(x => !entryIDSet.Contains(x.ItemID) && !mediaFileIDSet.Contains(x.MediaFileID)).Select(x => x.ItemID).ToHashSet();
                foreach (QueueEdit edit in RemovalEditList(leftSet, x => x.ItemID)) ApplyLocked(edit, outcome);

                return;
            }

            HashSet<Guid> goneSet = known.Where(x => !entryIDSet.Contains(x)).ToHashSet();
            foreach (QueueEdit edit in RemovalEditList(goneSet, x => x.ItemID)) ApplyLocked(edit, outcome);

            ShuffleModeEnum shuffle = await _serviceSource.GetShuffle(source);
            foreach (ResponsePlaylistEntry entry in detail.EntryList.Where(x => !known.Contains(x.ID)).OrderBy(x => x.Position))
            {
                QueuePlacement? placement = await _serviceSource.PlaceNew(source, shuffle, _queue, entry.MediaFileID, entry.ID);
                if (placement is not null) ApplyLocked(new() { Edit = QueueEditEnum.InsertSourceItem, Item = placement.Item, AfterItemID = placement.AfterItemID }, outcome);
            }
        });

        Publish(done);
    }

    // The edits that take what was deleted out of the queue: an upcoming item leaves, the playing one becomes the ghost and finishes. History is left alone.
    private List<QueueEdit> RemovalEditList(HashSet<Guid> goneSet, Func<QueueItem, Guid> keyOf)
    {
        List<QueueEdit> editList = [];
        if (goneSet.Count == 0) return editList;

        ResponseQueue queue = _queue.ToResponse();
        if (queue.Current is { } current && goneSet.Contains(keyOf(current))) _ghostItemID = current.ItemID;

        editList.AddRange(queue.UserQueueList.Where(x => goneSet.Contains(keyOf(x))).Select(x => new QueueEdit { Edit = QueueEditEnum.Remove, ItemID = x.ItemID }));
        editList.AddRange(queue.SourceList.Where(x => goneSet.Contains(keyOf(x))).Select(x => new QueueEdit { Edit = QueueEditEnum.RemoveSourceItem, ItemID = x.ItemID }));

        return editList;
    }

    #endregion -- The source changes underneath --

    #region -- Internals --

    private async Task<Outcome> Locked(Func<Outcome, Task> work)
    {
        Outcome outcome = new();
        await _lock.WaitAsync();
        try
        {
            await work(outcome);
        }
        finally
        {
            _lock.Release();
        }

        return outcome;
    }

    private void Publish(Outcome outcome)
    {
        foreach (QueueEdit edit in outcome.AppliedList) EditApplied?.Invoke(edit);

        if (outcome.Changed) Changed?.Invoke();
        if (outcome.CurrentRemoved) CurrentRemoved?.Invoke();
        if (outcome.Gap) GapDetected?.Invoke();
    }

    private void ApplyLocked(QueueEdit edit, Outcome outcome)
    {
        QueueItem? before = _queue.Current;
        ResponseQueueDelta? applied;
        lock (_stateLock) applied = _queue.Apply(edit, Constants.QueueHistoryLimit);
        if (applied is null) return;

        outcome.AppliedList.Add(edit);
        outcome.Changed = true;
        AfterChange(before, edit, outcome);
    }

    // What an applied edit means beyond the queue: the position starts again for a new current item, a removed current item makes the device skip, and a ghost that is no longer current is dropped.
    private void AfterChange(QueueItem? before, QueueEdit edit, Outcome outcome)
    {
        QueueItem? after = _queue.Current;
        if (edit.Edit == QueueEditEnum.Remove && before is not null && edit.ItemID == before.ItemID && after is null && OwnsPlayback) outcome.CurrentRemoved = true;
        if (before?.ItemID != after?.ItemID) _positionSeconds = 0;

        if (_ghostItemID is not { } ghost || after?.ItemID == ghost) return;

        _ghostItemID = null;
        QueueEdit drop = new() { Edit = QueueEditEnum.Remove, ItemID = ghost };
        ResponseQueueDelta? dropped;
        lock (_stateLock) dropped = _queue.Apply(drop, Constants.QueueHistoryLimit);
        if (dropped is not null) outcome.AppliedList.Add(drop);
    }

    private async Task SaveLocked()
    {
        await _storeLocalQueue.Save(new(_queue.ToResponse(), _positionSeconds, _timeProvider.GetUtcNow().UtcDateTime));
    }

    private static QueueEdit ToEdit(ResponseQueueDelta delta)
    {
        return new()
        {
            Edit = delta.Edit,
            ItemID = delta.ItemID,
            MediaFileID = delta.MediaFileID,
            AfterItemID = delta.AfterItemID,
            Item = delta.Item,
            ItemList = delta.ItemList is null ? null : [.. delta.ItemList],
            Source = delta.Source,
            CurrentItemID = delta.CurrentItemID,
            KeepUserQueue = delta.KeepUserQueue,
            KeepSkipped = delta.KeepSkipped
        };
    }

    private static bool Contains(ResponseQueue queue, Guid itemID)
    {
        return queue.Current?.ItemID == itemID || queue.HistoryList.Any(x => x.ItemID == itemID) || queue.UserQueueList.Any(x => x.ItemID == itemID) || queue.SourceList.Any(x => x.ItemID == itemID);
    }

    // This device is at least as far on as the snapshot (it is the echo of the claim this device made, and edits came after it): its own queue stays whole, and it takes the server's number. The echoes of the edits it already applied then come back as deltas that no longer fit, which asks for a snapshot and settles.
    private static ResponseQueue WithRevision(ResponseQueue local, long revision)
    {
        return new() { QueueRevision = revision, Source = local.Source, HistoryList = local.HistoryList, Current = local.Current, UserQueueList = local.UserQueueList, SourceList = local.SourceList };
    }

    // The snapshot's upcoming items, minus what this device already played or is playing; its own current item and history stay.
    private static ResponseQueue KeepLocalPointer(ResponseQueue local, QueueItem current, ResponseQueue snapshot)
    {
        HashSet<Guid> keptSet = [.. local.HistoryList.Select(x => x.ItemID), current.ItemID];

        return new()
        {
            QueueRevision = snapshot.QueueRevision,
            Source = snapshot.Source,
            HistoryList = local.HistoryList,
            Current = current,
            UserQueueList = snapshot.UserQueueList.Where(x => !keptSet.Contains(x.ItemID)).ToList(),
            SourceList = snapshot.SourceList.Where(x => !keptSet.Contains(x.ItemID)).ToList()
        };
    }

    #endregion -- Internals --
}
