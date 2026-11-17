using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Models;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;
using Noxy.NET.MediaPlayer.Domain.Responses.Remote;
using Noxy.NET.MediaPlayer.Presentation;
using Noxy.NET.MediaPlayer.Presentation.Models;
using Noxy.NET.MediaPlayer.Presentation.Services;
using Noxy.NET.Test.MediaPlayer.Fakes;
using Noxy.NET.Test.MediaPlayer.Fixtures;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class QueueServiceTest
{
    private readonly FakeMetadataStore _storeMetadata = new();
    private readonly FakeSettingService _serviceSetting = new();
    private readonly FakeLocalQueueStore _storeLocalQueue = new();
    private readonly FakeSyncService _serviceSync = new();
    private readonly FakeLogger<QueueService> _logger = new();
    private readonly List<QueueEdit> _editList = [];
    private readonly QueueService _service;
    private int _changedCount;

    public QueueServiceTest()
    {
        QueueSourceService serviceSource = new(_storeMetadata, _serviceSetting, new ShuffleService(new Random(21)), new Random(22));
        _service = new(serviceSource, _serviceSetting, _storeLocalQueue, _serviceSync, _storeMetadata, new FakeTimeProvider(), _logger);
        _service.EditApplied += _editList.Add;
        _service.Changed += () => _changedCount++;
    }

    private static ResponseQueueSource Library()
    {
        return new() { Kind = QueueSourceKindEnum.Library, PlaylistID = null, Sort = LibrarySortEnum.Title, Descending = false, Search = string.Empty };
    }

    private static ResponseQueueSource Playlist(Guid playlistID)
    {
        return new() { Kind = QueueSourceKindEnum.Playlist, PlaylistID = playlistID, Sort = LibrarySortEnum.Title, Descending = false, Search = string.Empty };
    }

    private async Task<ResponseMediaFile> AddMedia(string title)
    {
        ResponseMediaFile media = MediaFileFactory.Create(title);
        await _storeMetadata.SaveMediaFile(media);
        return media;
    }

    private async Task<List<ResponseMediaFile>> AddMedia(params string[] titleList)
    {
        List<ResponseMediaFile> mediaList = [];
        foreach (string title in titleList) mediaList.Add(await AddMedia(title));

        return mediaList;
    }

    private async Task<(Guid PlaylistID, List<ResponsePlaylistEntry> EntryList)> AddPlaylist(List<ResponseMediaFile> mediaList)
    {
        Guid playlistID = Guid.NewGuid();
        List<ResponsePlaylistEntry> entryList = mediaList.Select((media, index) => new ResponsePlaylistEntry { ID = Guid.NewGuid(), MediaFileID = media.ID, Position = index, MediaFile = media }).ToList();
        await SavePlaylist(playlistID, entryList);

        return (playlistID, entryList);
    }

    private async Task SavePlaylist(Guid playlistID, List<ResponsePlaylistEntry> entryList)
    {
        ResponsePlaylistDetail detail = new()
        {
            ID = playlistID, Name = "List", AllowDuplicates = true, EntryCount = entryList.Count, TimeCreated = DateTime.UtcNow, TimeModified = DateTime.UtcNow, TimeLastPlayed = null, EntryList = entryList
        };
        await _storeMetadata.SavePlaylist(detail);
        await _storeMetadata.SavePlaylistDetail(detail);
    }

    private async Task Start(Guid startMediaFileID, ShuffleModeEnum shuffle = ShuffleModeEnum.Off)
    {
        Assert.True(await _service.StartSource(Library(), startMediaFileID, shuffle));
    }

    private ResponseQueue Snapshot()
    {
        return _service.Queue.ToResponse();
    }

    private static List<Guid> MediaOf(List<QueueItem> itemList)
    {
        return itemList.Select(x => x.MediaFileID).ToList();
    }

    private QueueEdit PlayNextEdit(Guid mediaFileID)
    {
        return new() { Edit = QueueEditEnum.PlayNext, ItemID = Guid.NewGuid(), MediaFileID = mediaFileID };
    }

    private QueueEdit AdvanceEdit(QueueItem item)
    {
        return new() { Edit = QueueEditEnum.Advance, Item = item };
    }

    private ResponseQueueDelta Delta(QueueEditEnum edit, Guid? itemID = null, Guid? mediaFileID = null, QueueItem? item = null, long? revision = null)
    {
        return new() { QueueRevision = revision ?? _service.Queue.QueueRevision + 1, Edit = edit, ItemID = itemID, MediaFileID = mediaFileID, Item = item };
    }

    #region -- Apply --

    [Fact]
    public async Task Apply_PlayNext_PutsItAtTheFrontOfTheUserQueue_AndWritesNothingUntilTheCopyIsSaved()
    {
        List<ResponseMediaFile> mediaList = await AddMedia("A", "B", "C");
        await Start(mediaList[0].ID);
        _editList.Clear();
        int saved = _storeLocalQueue.SaveCount;
        QueueEdit edit = PlayNextEdit(mediaList[2].ID);

        Assert.True(await _service.Apply(edit));

        Assert.Equal([mediaList[2].ID], MediaOf(Snapshot().UserQueueList));
        Assert.Equal([edit], _editList);
        Assert.Equal(saved, _storeLocalQueue.SaveCount);
        await _service.SaveLocalCopy(12);
        LocalQueueSnapshot? local = await _storeLocalQueue.Get();
        Assert.Equal(Snapshot().QueueRevision, local?.Queue.QueueRevision);
        Assert.Equal([mediaList[2].ID], MediaOf(local?.Queue.UserQueueList ?? []));
    }

    [Fact]
    public async Task Apply_AnEditNamingAGoneItem_ChangesNothing()
    {
        List<ResponseMediaFile> mediaList = await AddMedia("A", "B");
        await Start(mediaList[0].ID);
        _editList.Clear();
        long revision = _service.Queue.QueueRevision;
        int saved = _storeLocalQueue.SaveCount;

        bool applied = await _service.Apply(new() { Edit = QueueEditEnum.Remove, ItemID = Guid.NewGuid() });

        Assert.False(applied);
        Assert.Equal(revision, _service.Queue.QueueRevision);
        Assert.Empty(_editList);
        Assert.Equal(saved, _storeLocalQueue.SaveCount);
    }

    [Fact]
    public async Task TheEchoOfItsOwnEdit_IsIgnored()
    {
        List<ResponseMediaFile> mediaList = await AddMedia("A", "B");
        await Start(mediaList[0].ID);
        QueueEdit edit = PlayNextEdit(mediaList[1].ID);
        await _service.Apply(edit);
        long revision = _service.Queue.QueueRevision;

        await _service.ApplyDelta(Delta(QueueEditEnum.PlayNext, edit.ItemID, edit.MediaFileID, revision: revision));

        Assert.Single(Snapshot().UserQueueList);
        Assert.Equal(revision, _service.Queue.QueueRevision);
    }

    #endregion -- Apply --

    #region -- StartSource and Regenerate --

    [Fact]
    public async Task StartSource_ReplacesTheQueue_WithTheBuiltSource_ClearsHistory()
    {
        List<ResponseMediaFile> mediaList = await AddMedia("A", "B", "C", "D");
        await Start(mediaList[0].ID);
        await _service.Apply(AdvanceEdit(Snapshot().SourceList[0]));
        Assert.NotEmpty(Snapshot().HistoryList);

        await Start(mediaList[1].ID);

        ResponseQueue queue = Snapshot();
        Assert.Empty(queue.HistoryList);
        Assert.Equal(mediaList[1].ID, queue.Current?.MediaFileID);
        Assert.Equal([mediaList[2].ID, mediaList[3].ID], MediaOf(queue.SourceList));
        Assert.Equal(QueueSourceKindEnum.Library, queue.Source?.Kind);
    }

    [Fact]
    public async Task StartSource_WithKeepQueuedItemsOn_KeepsTheUserQueueAfterTheNewCurrentItem()
    {
        _serviceSetting.KeepQueuedItems = true;
        List<ResponseMediaFile> mediaList = await AddMedia("A", "B", "C");
        await Start(mediaList[0].ID);
        await _service.Apply(PlayNextEdit(mediaList[2].ID));

        await Start(mediaList[1].ID);

        ResponseQueue queue = Snapshot();
        Assert.Equal(mediaList[1].ID, queue.Current?.MediaFileID);
        Assert.Equal([mediaList[2].ID], MediaOf(queue.UserQueueList));
    }

    [Fact]
    public async Task StartSource_WithKeepQueuedItemsOff_ClearsTheUserQueue()
    {
        _serviceSetting.KeepQueuedItems = false;
        List<ResponseMediaFile> mediaList = await AddMedia("A", "B", "C");
        await Start(mediaList[0].ID);
        await _service.Apply(PlayNextEdit(mediaList[2].ID));

        await Start(mediaList[1].ID);

        Assert.Empty(Snapshot().UserQueueList);
    }

    [Fact]
    public async Task StartSource_ForASourceThatCannotBeBuilt_ChangesNothing()
    {
        List<ResponseMediaFile> mediaList = await AddMedia("A", "B");
        await Start(mediaList[0].ID);
        _editList.Clear();
        long revision = _service.Queue.QueueRevision;
        int saved = _storeLocalQueue.SaveCount;

        Assert.False(await _service.StartSource(Playlist(Guid.NewGuid()), null, ShuffleModeEnum.Off));
        Assert.False(await _service.StartSource(Library(), Guid.NewGuid(), ShuffleModeEnum.Off));

        Assert.Equal(revision, _service.Queue.QueueRevision);
        Assert.Equal(mediaList[0].ID, Snapshot().Current?.MediaFileID);
        Assert.Empty(_editList);
        Assert.Equal(saved, _storeLocalQueue.SaveCount);
    }

    [Fact]
    public async Task StartSource_ForAnEmptySource_ChangesNothing()
    {
        Assert.False(await _service.StartSource(Library(), null, ShuffleModeEnum.Off));
        Assert.Null(Snapshot().Current);
    }

    [Fact]
    public async Task Regenerate_ReplacesOnlyTheSourcePart_KeepsHistoryCurrentAndTheUserQueue()
    {
        List<ResponseMediaFile> mediaList = await AddMedia("A", "B", "C", "D", "E", "F");
        ResponseMediaFile extra = await AddMedia("Z0");
        await Start(mediaList[0].ID);
        await _service.Apply(PlayNextEdit(extra.ID));
        await _service.Apply(AdvanceEdit(Snapshot().SourceList[0]));
        ResponseQueue before = Snapshot();

        await _service.Regenerate(ShuffleModeEnum.Random);

        ResponseQueue after = Snapshot();
        Assert.Equal(before.HistoryList, after.HistoryList);
        Assert.Equal(before.Current, after.Current);
        Assert.Equal(before.UserQueueList, after.UserQueueList);
        Assert.Equal(before.SourceList.Select(x => x.MediaFileID).Where(x => x != extra.ID).Order(), after.SourceList.Select(x => x.MediaFileID).Order());
    }

    [Fact]
    public async Task Regenerate_NeverRepeatsAnItemAlreadyInHistory()
    {
        List<ResponseMediaFile> mediaList = await AddMedia(Enumerable.Range(0, 12).Select(x => $"Media {x:00}").ToArray());
        await Start(mediaList[0].ID);
        for (int step = 0; step < 4; step++) await _service.Apply(AdvanceEdit(Snapshot().SourceList[0]));
        List<Guid> played = MediaOf(Snapshot().HistoryList);
        Assert.Equal(4, played.Count);

        await _service.Regenerate(ShuffleModeEnum.Random);

        ResponseQueue queue = Snapshot();
        Assert.DoesNotContain(queue.SourceList, x => played.Contains(x.MediaFileID) || x.MediaFileID == queue.Current?.MediaFileID);
        Assert.Equal(7, queue.SourceList.Count);
    }

    [Fact]
    public async Task Regenerate_WithNothingPlayingOrNoSource_ChangesNothing()
    {
        long revision = _service.Queue.QueueRevision;

        await _service.Regenerate(ShuffleModeEnum.Random);

        Assert.Equal(revision, _service.Queue.QueueRevision);
        Assert.Empty(_editList);
    }

    #endregion -- StartSource and Regenerate --

    #region -- Deltas and snapshots --

    [Fact]
    public async Task ApplyDelta_Plus1_IsApplied()
    {
        List<ResponseMediaFile> mediaList = await AddMedia("A", "B");
        await Start(mediaList[0].ID);
        long revision = _service.Queue.QueueRevision;
        _editList.Clear();

        await _service.ApplyDelta(Delta(QueueEditEnum.AddToQueue, Guid.NewGuid(), mediaList[1].ID));

        Assert.Equal([mediaList[1].ID], MediaOf(Snapshot().UserQueueList));
        Assert.Equal(revision + 1, _service.Queue.QueueRevision);
        Assert.Empty(_editList);
    }

    [Fact]
    public async Task ApplyDelta_AppliedThatChangesNothingHere_StillTakesTheServersRevision()
    {
        List<ResponseMediaFile> mediaList = await AddMedia("A", "B");
        await Start(mediaList[0].ID);
        long revision = _service.Queue.QueueRevision;

        await _service.ApplyDelta(Delta(QueueEditEnum.Remove, Guid.NewGuid()));

        Assert.Equal(revision + 1, _service.Queue.QueueRevision);
    }

    [Fact]
    public async Task ApplyDelta_LowerOrEqual_IsIgnored()
    {
        List<ResponseMediaFile> mediaList = await AddMedia("A", "B");
        await Start(mediaList[0].ID);
        long revision = _service.Queue.QueueRevision;

        await _service.ApplyDelta(Delta(QueueEditEnum.AddToQueue, Guid.NewGuid(), mediaList[1].ID, revision: revision));
        await _service.ApplyDelta(Delta(QueueEditEnum.AddToQueue, Guid.NewGuid(), mediaList[1].ID, revision: revision - 1));

        Assert.Empty(Snapshot().UserQueueList);
        Assert.Equal(revision, _service.Queue.QueueRevision);
    }

    [Fact]
    public async Task ApplyDelta_AGap_RaisesGapDetected_AndChangesNothing()
    {
        List<ResponseMediaFile> mediaList = await AddMedia("A", "B");
        await Start(mediaList[0].ID);
        long revision = _service.Queue.QueueRevision;
        int gapCount = 0;
        _service.GapDetected += () => gapCount++;

        await _service.ApplyDelta(Delta(QueueEditEnum.AddToQueue, Guid.NewGuid(), mediaList[1].ID, revision: revision + 2));

        Assert.Equal(1, gapCount);
        Assert.Empty(Snapshot().UserQueueList);
        Assert.Equal(revision, _service.Queue.QueueRevision);
    }

    [Fact]
    public async Task ApplyDelta_ThatIsNotValidHere_RaisesGapDetected()
    {
        int gapCount = 0;
        _service.GapDetected += () => gapCount++;

        await _service.ApplyDelta(Delta(QueueEditEnum.AddToQueue));

        Assert.Equal(1, gapCount);
    }

    private ResponseQueue Pushed(long revision, QueueItem? current, List<QueueItem> historyList, List<QueueItem> userList, List<QueueItem> sourceList, bool hasSource = true)
    {
        return new() { QueueRevision = revision, Source = hasSource ? Library() : null, HistoryList = historyList, Current = current, UserQueueList = userList, SourceList = sourceList };
    }

    private static QueueItem Item()
    {
        return new(Guid.NewGuid(), Guid.NewGuid());
    }

    [Fact]
    public async Task ReplaceWithSnapshot_OnAController_ReplacesTheCopy()
    {
        QueueItem current = Item();
        QueueItem next = Item();

        await _service.ReplaceWithSnapshot(Pushed(9, current, [], [], [next]), false);

        ResponseQueue queue = Snapshot();
        Assert.Equal(9, queue.QueueRevision);
        Assert.Equal(current, queue.Current);
        Assert.Equal([next], queue.SourceList);
        Assert.Equal(1, _changedCount);
        await _service.SaveLocalCopy(0);
        Assert.Equal(9, (await _storeLocalQueue.Get())?.Queue.QueueRevision);
    }

    [Fact]
    public async Task ReplaceWithSnapshot_OnTheActiveDevice_KeepsItsOwnCurrentAndHistory_WhenTheItemStillExists()
    {
        _service.OwnsPlayback = true;
        QueueItem a = Item();
        QueueItem b = Item();
        QueueItem c = Item();
        QueueItem d = Item();
        await _service.ReplaceWithSnapshot(Pushed(1, c, [a, b], [], [d]), false);
        int removedCount = 0;
        _service.CurrentRemoved += () => removedCount++;

        // The server is behind: it still has A playing.
        await _service.ReplaceWithSnapshot(Pushed(5, a, [], [], [b, c, d]), true);

        ResponseQueue queue = Snapshot();
        Assert.Equal(c, queue.Current);
        Assert.Equal([a, b], queue.HistoryList);
        Assert.Equal([d], queue.SourceList);
        Assert.Equal(5, queue.QueueRevision);
        Assert.Equal(0, removedCount);
    }

    [Fact]
    public async Task ReplaceWithSnapshot_OnTheActiveDevice_AStaleSnapshotThatLacksTheCurrentItem_KeepsThePointerAndTheHistory()
    {
        _service.OwnsPlayback = true;
        QueueItem a = Item();
        QueueItem b = Item();
        QueueItem c = Item();
        await _service.ReplaceWithSnapshot(Pushed(5, c, [a, b], [], []), false);
        int removedCount = 0;
        _service.CurrentRemoved += () => removedCount++;

        // The snapshot was made before this device played on: it still has A playing and knows nothing of B or C.
        await _service.ReplaceWithSnapshot(Pushed(2, a, [], [], []), true);

        ResponseQueue queue = Snapshot();
        Assert.Equal(c, queue.Current);
        Assert.Equal([a, b], queue.HistoryList);
        Assert.Equal(0, removedCount);
        Assert.Equal(2, queue.QueueRevision);
    }

    [Fact]
    public async Task ReplaceWithSnapshot_OnTheActiveDevice_WhenItIsAheadOfTheSnapshot_KeepsItsWholeQueue_AndTakesTheRevision()
    {
        _service.OwnsPlayback = true;
        QueueItem current = Item();
        QueueItem queued = Item();
        QueueItem next = Item();
        await _service.ReplaceWithSnapshot(Pushed(4, current, [], [queued], [next]), false);

        // The echo of this device's own claim: made before it queued something.
        await _service.ReplaceWithSnapshot(Pushed(3, current, [], [], [next]), true);

        ResponseQueue queue = Snapshot();
        Assert.Equal([queued], queue.UserQueueList);
        Assert.Equal([next], queue.SourceList);
        Assert.Equal(3, queue.QueueRevision);
    }

    [Fact]
    public async Task ASnapshotWithACurrentRemoved_MakesTheActiveDeviceSkip()
    {
        _service.OwnsPlayback = true;
        QueueItem a = Item();
        QueueItem b = Item();
        await _service.ReplaceWithSnapshot(Pushed(1, a, [], [], [b]), false);
        int removedCount = 0;
        _service.CurrentRemoved += () => removedCount++;

        await _service.ReplaceWithSnapshot(Pushed(2, null, [], [], [b]), true);

        Assert.Equal(1, removedCount);
        Assert.Null(Snapshot().Current);
    }

    #endregion -- Deltas and snapshots --

    #region -- Removing the current item --

    [Fact]
    public async Task ARemoveOfTheCurrentItem_FromAController_MakesTheActiveDeviceSkipAtOnce_AndItNeverEntersHistory()
    {
        _service.OwnsPlayback = true;
        List<ResponseMediaFile> mediaList = await AddMedia("A", "B", "C");
        await Start(mediaList[0].ID);
        ResponseQueue before = Snapshot();
        QueueItem? next = before.SourceList.FirstOrDefault();
        _service.CurrentRemoved += () => _ = _service.Apply(AdvanceEdit(next ?? throw new InvalidOperationException("Nothing next.")));

        await _service.ApplyDelta(Delta(QueueEditEnum.Remove, before.Current?.ItemID));

        ResponseQueue after = Snapshot();
        Assert.Equal(next, after.Current);
        Assert.Empty(after.HistoryList);
    }

    [Fact]
    public async Task ARemoveOfTheCurrentItem_OnADeviceThatDoesNotPlay_RaisesNothing()
    {
        List<ResponseMediaFile> mediaList = await AddMedia("A", "B");
        await Start(mediaList[0].ID);
        int removedCount = 0;
        _service.CurrentRemoved += () => removedCount++;

        await _service.ApplyDelta(Delta(QueueEditEnum.Remove, Snapshot().Current?.ItemID));

        Assert.Equal(0, removedCount);
    }

    [Fact]
    public async Task ARemoveRacingAJustStartedAdvance_EndsWithTheDeviceOnTheNextItem_AndNoError()
    {
        _service.OwnsPlayback = true;
        List<ResponseMediaFile> mediaList = await AddMedia("A", "B", "C");
        await Start(mediaList[0].ID);
        ResponseQueue start = Snapshot();
        QueueItem a = start.Current ?? throw new InvalidOperationException("No current.");
        QueueItem b = start.SourceList[0];
        long revision = _service.Queue.QueueRevision;
        int removedCount = 0;
        _service.CurrentRemoved += () => removedCount++;

        // The device advanced to B on its own; the controller's Remove of A reaches the server first, so the pushes carry the same revision numbers in the other order.
        await _service.Apply(AdvanceEdit(b));
        await _service.ApplyDelta(Delta(QueueEditEnum.Remove, a.ItemID, revision: revision + 1));
        await _service.ApplyDelta(Delta(QueueEditEnum.Advance, item: b, revision: revision + 2));

        Assert.Equal(b, Snapshot().Current);
        Assert.Equal(revision + 2, _service.Queue.QueueRevision);
        Assert.Equal(0, removedCount);
    }

    #endregion -- Removing the current item --

    #region -- The source changes underneath --

    private async Task StartAt(ResponseMediaFile start)
    {
        _service.OwnsPlayback = true;
        await Start(start.ID);
    }

    [Fact]
    public async Task AnUploadedMediaFile_ForASortedLibraryQueue_IsInsertedOnlyWhenAfterTheCurrentItem()
    {
        List<ResponseMediaFile> mediaList = await AddMedia("A", "C", "E");
        await StartAt(mediaList[1]);
        ResponseMediaFile b = await AddMedia("B");
        ResponseMediaFile d = await AddMedia("D");
        _editList.Clear();

        _serviceSync.RaiseLibraryDiff(new([b], [], []));
        Assert.Equal([mediaList[2].ID], MediaOf(Snapshot().SourceList));

        _serviceSync.RaiseLibraryDiff(new([d], [], []));

        Assert.Equal([d.ID, mediaList[2].ID], MediaOf(Snapshot().SourceList));
        Assert.Equal([QueueEditEnum.InsertSourceItem], _editList.Select(x => x.Edit));
    }

    [Fact]
    public async Task AnUploadedMediaFile_OnAControllerOrWithNoSource_ChangesNothing()
    {
        List<ResponseMediaFile> mediaList = await AddMedia("A", "C");
        _service.OwnsPlayback = false;
        await Start(mediaList[0].ID);
        ResponseMediaFile d = await AddMedia("D");
        _editList.Clear();

        _serviceSync.RaiseLibraryDiff(new([d], [], []));
        Assert.Equal([mediaList[1].ID], MediaOf(Snapshot().SourceList));

        _service.OwnsPlayback = true;
        await _service.ReplaceWithSnapshot(Pushed(3, Item(), [], [], [], false), false);
        _serviceSync.RaiseLibraryDiff(new([d], [], []));

        Assert.Empty(Snapshot().SourceList);
        Assert.Empty(_editList);
    }

    [Fact]
    public async Task ADeletedUpcomingItem_IsRemovedFromTheQueue()
    {
        List<ResponseMediaFile> mediaList = await AddMedia("A", "B", "C");
        await StartAt(mediaList[0]);
        await _service.Apply(PlayNextEdit(mediaList[2].ID));
        _editList.Clear();

        _serviceSync.RaiseLibraryDiff(new([], [], [mediaList[2].ID]));

        ResponseQueue queue = Snapshot();
        Assert.Equal([mediaList[1].ID], MediaOf(queue.SourceList));
        Assert.Empty(queue.UserQueueList);
        Assert.Equal(mediaList[0].ID, queue.Current?.MediaFileID);
    }

    [Fact]
    public async Task ADeletedCurrentItem_BecomesAGhost_AndIsDroppedWithoutEnteringHistory_OnAdvance()
    {
        List<ResponseMediaFile> mediaList = await AddMedia("A", "B", "C");
        await StartAt(mediaList[0]);
        ResponseQueue start = Snapshot();
        QueueItem ghost = start.Current ?? throw new InvalidOperationException("No current.");
        int removedCount = 0;
        _service.CurrentRemoved += () => removedCount++;

        _serviceSync.RaiseLibraryDiff(new([], [], [mediaList[0].ID]));

        Assert.Equal(ghost, Snapshot().Current);
        Assert.Equal(0, removedCount);
        _editList.Clear();

        await _service.Apply(AdvanceEdit(start.SourceList[0]));

        ResponseQueue after = Snapshot();
        Assert.Equal(start.SourceList[0], after.Current);
        Assert.Empty(after.HistoryList);
        Assert.DoesNotContain(after.SourceList, x => x == ghost);
        Assert.Equal([QueueEditEnum.Advance, QueueEditEnum.Remove], _editList.Select(x => x.Edit));
    }

    [Fact]
    public async Task ADeletedPlaylist_EndsTheSourcePart_AndLetsTheCurrentItemFinish()
    {
        List<ResponseMediaFile> mediaList = await AddMedia("A", "B", "C");
        (Guid playlistID, List<ResponsePlaylistEntry> entryList) = await AddPlaylist(mediaList);
        _service.OwnsPlayback = true;
        Assert.True(await _service.StartSource(Playlist(playlistID), entryList[0].ID, ShuffleModeEnum.Off));

        await _storeMetadata.DeletePlaylist(playlistID);
        _serviceSync.RaiseChanged();

        ResponseQueue queue = Snapshot();
        Assert.Equal(entryList[0].ID, queue.Current?.ItemID);
        Assert.Empty(queue.SourceList);
    }

    [Fact]
    public async Task AnEntryRemovedFromThePlaylist_LeavesTheQueue_AndOneAddedJoinsIt()
    {
        List<ResponseMediaFile> mediaList = await AddMedia("A", "B", "C");
        (Guid playlistID, List<ResponsePlaylistEntry> entryList) = await AddPlaylist(mediaList);
        _service.OwnsPlayback = true;
        Assert.True(await _service.StartSource(Playlist(playlistID), entryList[0].ID, ShuffleModeEnum.Off));
        ResponseMediaFile added = await AddMedia("D");
        ResponsePlaylistEntry newEntry = new() { ID = Guid.NewGuid(), MediaFileID = added.ID, Position = 3, MediaFile = added };

        await SavePlaylist(playlistID, [entryList[0], entryList[2], newEntry]);
        _serviceSync.RaiseChanged();

        Assert.Equal([entryList[2].ID, newEntry.ID], Snapshot().SourceList.Select(x => x.ItemID));
    }

    [Fact]
    public async Task AnEntryThatLeftThePlaylist_WhileTheDeviceWasAway_LeavesTheQueueAtTheFirstLookAfterALoad()
    {
        List<ResponseMediaFile> mediaList = await AddMedia("A", "B", "C");
        (Guid playlistID, List<ResponsePlaylistEntry> entryList) = await AddPlaylist(mediaList);
        _service.OwnsPlayback = true;
        Assert.True(await _service.StartSource(Playlist(playlistID), entryList[0].ID, ShuffleModeEnum.Off));
        await _service.SaveLocalCopy(0);

        FakeSyncService sync = new();
        QueueService restored = new(new QueueSourceService(_storeMetadata, _serviceSetting, new ShuffleService(new Random(1)), new Random(1)), _serviceSetting, _storeLocalQueue, sync, _storeMetadata, new FakeTimeProvider(), new FakeLogger<QueueService>())
            { OwnsPlayback = true };
        await restored.LoadLocalCopy();
        await SavePlaylist(playlistID, [entryList[0], entryList[2]]);
        await _storeMetadata.DeleteMediaFile(mediaList[1].ID);

        sync.RaiseChanged();

        Assert.Equal([entryList[2].ID], restored.Queue.ToResponse().SourceList.Select(x => x.ItemID));
    }

    [Fact]
    public async Task TheFirstLookAfterALoad_NeverTreatsAQueuedPlayingItemAsGone_SoItStillEntersHistory()
    {
        List<ResponseMediaFile> mediaList = await AddMedia("A", "B", "C");
        ResponseMediaFile queued = await AddMedia("Queued");
        (Guid playlistID, List<ResponsePlaylistEntry> entryList) = await AddPlaylist(mediaList);
        _service.OwnsPlayback = true;
        Assert.True(await _service.StartSource(Playlist(playlistID), entryList[0].ID, ShuffleModeEnum.Off));
        await _service.Apply(PlayNextEdit(queued.ID));
        // A snapshot forgets what is known of the playlist, as the echo of a claim does.
        await _service.ReplaceWithSnapshot(Snapshot(), true);
        await _service.Apply(AdvanceEdit(Snapshot().UserQueueList[0]));

        _serviceSync.RaiseChanged();
        await _service.Apply(AdvanceEdit(Snapshot().SourceList[0]));

        Assert.Equal([entryList[0].ID, Snapshot().HistoryList[1].ItemID], Snapshot().HistoryList.Select(x => x.ItemID));
        Assert.Equal(queued.ID, Snapshot().HistoryList[1].MediaFileID);
    }

    [Fact]
    public async Task AnEntryTheUserRemovedFromTheQueue_IsNotPutBackBySyncing()
    {
        List<ResponseMediaFile> mediaList = await AddMedia("A", "B", "C");
        (Guid playlistID, List<ResponsePlaylistEntry> entryList) = await AddPlaylist(mediaList);
        _service.OwnsPlayback = true;
        Assert.True(await _service.StartSource(Playlist(playlistID), entryList[0].ID, ShuffleModeEnum.Random));
        QueueItem removed = Snapshot().SourceList[0];
        await _service.Apply(new() { Edit = QueueEditEnum.Remove, ItemID = removed.ItemID });

        _serviceSync.RaiseChanged();

        Assert.DoesNotContain(Snapshot().SourceList, x => x == removed);
    }

    #endregion -- The source changes underneath --

    #region -- The local copy --

    [Fact]
    public async Task LoadLocalCopy_RestoresTheQueue()
    {
        List<ResponseMediaFile> mediaList = await AddMedia("A", "B", "C");
        await Start(mediaList[0].ID);
        await _service.Apply(PlayNextEdit(mediaList[2].ID));
        await _service.SaveLocalCopy(41.5);
        ResponseQueue saved = Snapshot();

        QueueService other = new(new QueueSourceService(_storeMetadata, _serviceSetting, new ShuffleService(new Random(1)), new Random(1)), _serviceSetting, _storeLocalQueue, new FakeSyncService(), _storeMetadata, new FakeTimeProvider(),
            new FakeLogger<QueueService>());
        TimeSpan? position = await other.LoadLocalCopy();

        Assert.Equal(TimeSpan.FromSeconds(41.5), position);
        ResponseQueue loaded = other.Queue.ToResponse();
        Assert.Equal(saved.Current, loaded.Current);
        Assert.Equal(saved.UserQueueList, loaded.UserQueueList);
        Assert.Equal(saved.SourceList, loaded.SourceList);
        Assert.Equal(saved.QueueRevision, loaded.QueueRevision);
    }

    [Fact]
    public async Task LoadLocalCopy_WithNothingSaved_ReturnsNull()
    {
        Assert.Null(await _service.LoadLocalCopy());
        Assert.Equal(0, _changedCount);
    }

    [Fact]
    public async Task LoadLocalCopy_OfACopyThatIsNotAValidQueue_IsDiscarded()
    {
        QueueItem duplicate = Item();
        await _storeLocalQueue.Save(new(Pushed(1, duplicate, [], [], [duplicate]), 0, DateTime.UtcNow));

        Assert.Null(await _service.LoadLocalCopy());
        Assert.Null(await _storeLocalQueue.Get());
    }

    [Fact]
    public async Task Clear_EmptiesTheQueue_AndTheLocalCopy()
    {
        List<ResponseMediaFile> mediaList = await AddMedia("A", "B");
        await Start(mediaList[0].ID);

        await _service.Clear();

        Assert.Null(Snapshot().Current);
        Assert.Equal(0, _service.Queue.ItemCount);
        Assert.Null(await _storeLocalQueue.Get());
    }

    [Fact]
    public async Task AnEditOfAFullQueue_WritesNothing_AndTheCopyIsWrittenOnceWhenSaved()
    {
        List<ResponseMediaFile> mediaList = Enumerable.Range(0, Constants.QueueMaxItemCount).Select(index => MediaFileFactory.Create($"Media {index:00000}")).ToList();
        await _storeMetadata.ReplaceMediaFileList(mediaList);
        await Start(mediaList[0].ID);
        int saved = _storeLocalQueue.SaveCount;

        await _service.Apply(PlayNextEdit(mediaList[5].ID));

        Assert.Equal(saved, _storeLocalQueue.SaveCount);

        await _service.SaveLocalCopy(0);

        Assert.Equal(saved + 1, _storeLocalQueue.SaveCount);
        Assert.Equal(Constants.QueueMaxItemCount - 1, Snapshot().SourceList.Count);
    }

    #endregion -- The local copy --
}
