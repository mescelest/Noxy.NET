using System.Diagnostics;
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

public class QueueSourceServiceTest
{
    private readonly FakeMetadataStore _storeMetadata = new();
    private readonly FakeSettingService _serviceSetting = new();
    private readonly QueueSourceService _service;

    public QueueSourceServiceTest()
    {
        _service = new(_storeMetadata, new ShuffleService(new Random(11)), new Random(12));
    }

    private static ResponseQueueSource Library(LibrarySortEnum sort = LibrarySortEnum.Title, bool descending = false, string search = "")
    {
        return new() { Kind = QueueSourceKindEnum.Library, PlaylistID = null, Sort = sort, Descending = descending, Search = search };
    }

    private static ResponseQueueSource Playlist(Guid playlistID)
    {
        return new() { Kind = QueueSourceKindEnum.Playlist, PlaylistID = playlistID, Sort = LibrarySortEnum.Title, Descending = false, Search = string.Empty };
    }

    private async Task<ResponseMediaFile> AddMedia(string title, string? artist = null, int? year = null)
    {
        ResponseMediaFile media = MediaFileFactory.Create(title, artist, year: year);
        await _storeMetadata.SaveMediaFile(media);
        return media;
    }

    private async Task<(Guid PlaylistID, List<ResponsePlaylistEntry> EntryList)> AddPlaylist(List<ResponseMediaFile> mediaList, bool holdDetail = true, bool inList = true)
    {
        Guid playlistID = Guid.NewGuid();
        List<ResponsePlaylistEntry> entryList = mediaList.Select((media, index) => new ResponsePlaylistEntry { ID = Guid.NewGuid(), MediaFileID = media.ID, Position = index, MediaFile = media }).ToList();
        ResponsePlaylistDetail detail = new()
        {
            ID = playlistID, Name = "List", AllowDuplicates = true, EntryCount = entryList.Count, TimeCreated = DateTime.UtcNow, TimeModified = DateTime.UtcNow, TimeLastPlayed = null, EntryList = entryList
        };

        if (inList) await _storeMetadata.SavePlaylist(detail);
        if (holdDetail) await _storeMetadata.SavePlaylistDetail(detail);
        return (playlistID, entryList);
    }

    private static List<Guid> MediaOf(List<QueueItem>? itemList)
    {
        return (itemList ?? throw new InvalidOperationException("No list.")).Select(x => x.MediaFileID).ToList();
    }

    private static List<QueueItem> Have(List<QueueItem>? itemList)
    {
        return itemList ?? throw new InvalidOperationException("The source was not built.");
    }

    private static QueuePlacement HavePlacement(QueuePlacement? placement)
    {
        return placement ?? throw new InvalidOperationException("The item was not placed.");
    }

    [Fact]
    public async Task BuildBase_FiltersBySearch_AndSortsByTheChosenKey_BothDirections()
    {
        ResponseMediaFile b = await AddMedia("Bravo", "Zed");
        ResponseMediaFile a = await AddMedia("Alpha", "Yan");
        ResponseMediaFile c = await AddMedia("Charlie bravo", "Xi");
        ResponseMediaFile d = await AddMedia("Delta", "Wu");

        Assert.Equal([a.ID, b.ID, c.ID, d.ID], MediaOf(await _service.BuildBase(Library())));
        Assert.Equal([d.ID, c.ID, b.ID, a.ID], MediaOf(await _service.BuildBase(Library(descending: true))));
        Assert.Equal([b.ID, c.ID], MediaOf(await _service.BuildBase(Library(search: "bravo"))));
        Assert.Equal([c.ID, b.ID], MediaOf(await _service.BuildBase(Library(search: "bravo", descending: true))));
        Assert.Equal([d.ID, c.ID, a.ID, b.ID], MediaOf(await _service.BuildBase(Library(LibrarySortEnum.Artist))));
    }

    [Fact]
    public async Task BuildBase_BreaksTiesByID_SoTheOrderDoesNotDependOnTheStoreOrder()
    {
        ResponseMediaFile first = MediaFileFactory.Create("Same", id: Guid.Parse("00000000-0000-0000-0000-000000000001"));
        ResponseMediaFile second = MediaFileFactory.Create("Same", id: Guid.Parse("00000000-0000-0000-0000-000000000002"));
        FakeMetadataStore other = new();
        await _storeMetadata.SaveMediaFile(second);
        await _storeMetadata.SaveMediaFile(first);
        await other.SaveMediaFile(first);
        await other.SaveMediaFile(second);

        List<Guid> one = MediaOf(await _service.BuildBase(Library()));
        List<Guid> two = MediaOf(await new QueueSourceService(other, new ShuffleService(new Random(1)), new Random(1)).BuildBase(Library()));

        Assert.Equal([first.ID, second.ID], one);
        Assert.Equal(one, two);
    }

    [Fact]
    public async Task BuildBase_ForAnEmptySearch_IncludesEveryMediaFile()
    {
        await AddMedia("One");
        await AddMedia("Two");
        await AddMedia("Three");

        Assert.Equal(3, Have(await _service.BuildBase(Library(search: ""))).Count);
    }

    [Fact]
    public async Task BuildBase_ForALibrary_GivesEveryItemItsOwnNewItemID()
    {
        await AddMedia("One");

        List<QueueItem> first = Have(await _service.BuildBase(Library()));
        List<QueueItem> second = Have(await _service.BuildBase(Library()));

        Assert.NotEqual(first[0].ItemID, second[0].ItemID);
        Assert.NotEqual(Guid.Empty, first[0].ItemID);
    }

    [Fact]
    public async Task BuildBase_ReturnsTheEntriesByPosition_WithTheEntryIDsAsItemIDs()
    {
        ResponseMediaFile one = await AddMedia("One");
        ResponseMediaFile two = await AddMedia("Two");
        (Guid playlistID, List<ResponsePlaylistEntry> entryList) = await AddPlaylist([two, one]);

        List<QueueItem> result = Have(await _service.BuildBase(Playlist(playlistID)));

        Assert.Equal(entryList.Select(x => x.ID), result.Select(x => x.ItemID));
        Assert.Equal([two.ID, one.ID], MediaOf(result));
    }

    [Fact]
    public async Task BuildBase_KeepsAMediaFileThatIsInThePlaylistTwice_AsTwoItems()
    {
        ResponseMediaFile one = await AddMedia("One");
        (Guid playlistID, _) = await AddPlaylist([one, one]);

        List<QueueItem> result = Have(await _service.BuildBase(Playlist(playlistID)));

        Assert.Equal(2, result.Select(x => x.ItemID).Distinct().Count());
    }

    [Fact]
    public async Task Build_ForAPlaylistNotInTheLocalListOrWithoutItsDetail_IsNull()
    {
        ResponseMediaFile one = await AddMedia("One");
        (Guid notListed, _) = await AddPlaylist([one], inList: false);
        (Guid noDetail, _) = await AddPlaylist([one], holdDetail: false);

        Assert.Null(await _service.Build(Playlist(Guid.NewGuid()), ShuffleModeEnum.Off, null));
        Assert.Null(await _service.Build(Playlist(notListed), ShuffleModeEnum.Off, null));
        Assert.Null(await _service.Build(Playlist(noDetail), ShuffleModeEnum.Off, null));
    }

    [Fact]
    public async Task Build_WithAStartThatIsNotInTheSource_IsNull()
    {
        await AddMedia("One");

        Assert.Null(await _service.Build(Library(), ShuffleModeEnum.Off, Guid.NewGuid()));
    }

    [Fact]
    public async Task Build_WithShuffleOff_StartsAtTheStartItem()
    {
        ResponseMediaFile a = await AddMedia("A");
        ResponseMediaFile b = await AddMedia("B");
        ResponseMediaFile c = await AddMedia("C");

        Assert.Equal([b.ID, c.ID], MediaOf(await _service.Build(Library(), ShuffleModeEnum.Off, b.ID)));
        Assert.Equal([a.ID, b.ID, c.ID], MediaOf(await _service.Build(Library(), ShuffleModeEnum.Off, null)));
    }

    [Fact]
    public async Task Build_WithShuffle_PutsTheStartItemFirst_ThenTheRest_NoneLostOrDuplicated()
    {
        List<ResponseMediaFile> mediaList = [];
        for (int index = 0; index < 30; index++) mediaList.Add(await AddMedia($"Media {index:00}"));
        (Guid playlistID, List<ResponsePlaylistEntry> entryList) = await AddPlaylist(mediaList);

        List<QueueItem> library = Have(await _service.Build(Library(), ShuffleModeEnum.Random, mediaList[7].ID));
        List<QueueItem> playlist = Have(await _service.Build(Playlist(playlistID), ShuffleModeEnum.Random, entryList[7].ID));

        Assert.Equal(mediaList[7].ID, library[0].MediaFileID);
        Assert.Equal(30, library.Select(x => x.MediaFileID).Distinct().Count());
        Assert.Equal(entryList[7].ID, playlist[0].ItemID);
        Assert.Equal(entryList.Select(x => x.ID).Order(), playlist.Select(x => x.ItemID).Order());
    }

    [Fact]
    public async Task Regenerate_NeverIncludesTheCurrentItemOrTheExcludedOnes()
    {
        List<ResponseMediaFile> mediaList = [];
        for (int index = 0; index < 20; index++) mediaList.Add(await AddMedia($"Media {index:00}"));
        (Guid playlistID, List<ResponsePlaylistEntry> entryList) = await AddPlaylist(mediaList);
        QueueItem current = new(entryList[5].ID, entryList[5].MediaFileID);
        List<QueueItem> excluded = [new(entryList[1].ID, entryList[1].MediaFileID), new(entryList[9].ID, entryList[9].MediaFileID)];

        List<QueueItem> result = Have(await _service.Regenerate(Playlist(playlistID), ShuffleModeEnum.Random, current, excluded));

        Assert.Equal(17, result.Count);
        Assert.DoesNotContain(result, x => x.ItemID == current.ItemID || excluded.Any(e => e.ItemID == x.ItemID));

        List<QueueItem> library = Have(await _service.Regenerate(Library(), ShuffleModeEnum.Random, new(Guid.NewGuid(), mediaList[5].ID), [new(Guid.NewGuid(), mediaList[1].ID)]));
        Assert.Equal(18, library.Count);
        Assert.DoesNotContain(library, x => x.MediaFileID == mediaList[5].ID || x.MediaFileID == mediaList[1].ID);
    }

    [Fact]
    public async Task Regenerate_WithShuffleOff_ContinuesInTheSourcesOrderAfterTheCurrentItem()
    {
        ResponseMediaFile a = await AddMedia("A");
        ResponseMediaFile b = await AddMedia("B");
        ResponseMediaFile c = await AddMedia("C");
        ResponseMediaFile d = await AddMedia("D");

        List<QueueItem> result = Have(await _service.Regenerate(Library(), ShuffleModeEnum.Off, new(Guid.NewGuid(), b.ID), [new(Guid.NewGuid(), d.ID)]));

        Assert.Equal([c.ID], MediaOf(result));
        Assert.DoesNotContain(a.ID, MediaOf(result));
    }

    [Fact]
    public async Task Regenerate_ForASourceThatCannotBeBuilt_IsNull()
    {
        Assert.Null(await _service.Regenerate(Playlist(Guid.NewGuid()), ShuffleModeEnum.Off, new(Guid.NewGuid(), Guid.NewGuid()), []));
    }

    private static LiveQueue QueueOf(QueueItem? current, List<QueueItem> sourceList, List<QueueItem>? historyList = null)
    {
        return LiveQueue.From(new()
        {
            QueueRevision = 1, Source = null, HistoryList = historyList ?? [], Current = current, UserQueueList = [], SourceList = sourceList
        }, 100);
    }

    [Fact]
    public async Task PlaceNew_ForASortedLibrary_OnlyWhenTheSortPutsItAfterTheCurrentItem()
    {
        ResponseMediaFile a = await AddMedia("A");
        ResponseMediaFile c = await AddMedia("C");
        ResponseMediaFile e = await AddMedia("E");
        QueueItem itemA = new(Guid.NewGuid(), a.ID);
        QueueItem itemC = new(Guid.NewGuid(), c.ID);
        QueueItem itemE = new(Guid.NewGuid(), e.ID);
        LiveQueue queue = QueueOf(itemC, [itemE], [itemA]);

        ResponseMediaFile b = await AddMedia("B");
        ResponseMediaFile d = await AddMedia("D");
        ResponseMediaFile f = await AddMedia("F");

        Assert.Null(await _service.PlaceNew(Library(), ShuffleModeEnum.Off, queue, b.ID));

        QueuePlacement afterCurrent = HavePlacement(await _service.PlaceNew(Library(), ShuffleModeEnum.Off, queue, d.ID));
        Assert.Null(afterCurrent.AfterItemID);
        Assert.Equal(d.ID, afterCurrent.Item.MediaFileID);

        QueuePlacement last = HavePlacement(await _service.PlaceNew(Library(), ShuffleModeEnum.Off, queue, f.ID));
        Assert.Equal(itemE.ItemID, last.AfterItemID);
    }

    [Fact]
    public async Task PlaceNew_ForAShuffledQueue_IsAfterTheCurrentItem()
    {
        ResponseMediaFile a = await AddMedia("A");
        ResponseMediaFile b = await AddMedia("B");
        ResponseMediaFile c = await AddMedia("C");
        QueueItem current = new(Guid.NewGuid(), a.ID);
        List<QueueItem> sourceList = [new(Guid.NewGuid(), c.ID), new(Guid.NewGuid(), b.ID)];
        LiveQueue queue = QueueOf(current, sourceList);
        ResponseMediaFile added = await AddMedia("Z");

        QueuePlacement placement = HavePlacement(await _service.PlaceNew(Library(), ShuffleModeEnum.Random, queue, added.ID));

        Assert.True(placement.AfterItemID is null || sourceList.Any(x => x.ItemID == placement.AfterItemID));
        Assert.NotEqual(current.ItemID, placement.AfterItemID);
    }

    [Fact]
    public async Task PlaceNew_WithNothingPlaying_IsNull()
    {
        ResponseMediaFile added = await AddMedia("Z");

        Assert.Null(await _service.PlaceNew(Library(), ShuffleModeEnum.Off, QueueOf(null, []), added.ID));
    }

    [Fact]
    public async Task PlaceNew_ForAPlaylist_GoesAfterTheEntryThatPrecedesItInThePlaylist()
    {
        ResponseMediaFile one = await AddMedia("One");
        ResponseMediaFile two = await AddMedia("Two");
        ResponseMediaFile three = await AddMedia("Three");
        (Guid playlistID, List<ResponsePlaylistEntry> entryList) = await AddPlaylist([one, two, three]);
        QueueItem current = new(entryList[0].ID, one.ID);
        QueueItem last = new(entryList[2].ID, three.ID);

        // Entry two was added to the playlist after the queue was made.
        QueuePlacement placement = HavePlacement(await _service.PlaceNew(Playlist(playlistID), ShuffleModeEnum.Off, QueueOf(current, [last]), two.ID));

        Assert.Equal(entryList[1].ID, placement.Item.ItemID);
        Assert.Null(placement.AfterItemID);
    }

    [Theory]
    [InlineData(ShuffleModeEnum.Off)]
    [InlineData(ShuffleModeEnum.Random)]
    public async Task Build_OfALibraryBiggerThanTheQueueLimit_IsCutToTheLimit_WithTheStartItemFirst(ShuffleModeEnum shuffle)
    {
        List<ResponseMediaFile> mediaList = Enumerable.Range(0, PresentationConstants.QueueMaxItemCount + 200).Select(index => MediaFileFactory.Create($"Media {index:00000}")).ToList();
        await _storeMetadata.ReplaceMediaFileList(mediaList);

        List<QueueItem> built = Have(await _service.Build(Library(), shuffle, mediaList[10].ID));

        Assert.Equal(PresentationConstants.QueueMaxItemCount, built.Count);
        Assert.Equal(mediaList[10].ID, built[0].MediaFileID);
        Assert.Equal(built.Count, built.Select(x => x.MediaFileID).Distinct().Count());
        if (shuffle == ShuffleModeEnum.Off) Assert.Equal(mediaList[PresentationConstants.QueueMaxItemCount + 9].ID, built[^1].MediaFileID);
    }

    [Fact]
    public async Task Regenerate_OfALibraryBiggerThanTheQueueLimit_KeepsTheWholeQueueWithinTheLimit()
    {
        List<ResponseMediaFile> mediaList = Enumerable.Range(0, PresentationConstants.QueueMaxItemCount + 200).Select(index => MediaFileFactory.Create($"Media {index:00000}")).ToList();
        await _storeMetadata.ReplaceMediaFileList(mediaList);
        List<QueueItem> excluded = mediaList.Take(40).Select(x => new QueueItem(Guid.NewGuid(), x.ID)).ToList();

        List<QueueItem> built = Have(await _service.Regenerate(Library(), ShuffleModeEnum.Random, new(Guid.NewGuid(), mediaList[50].ID), excluded));

        Assert.Equal(PresentationConstants.QueueMaxItemCount - excluded.Count - 1, built.Count);
    }

    [Fact]
    public async Task Build_OfALibraryOf20000_CompletesInASinglePass_AndIsCutToTheQueueLimit()
    {
        List<ResponseMediaFile> mediaList = Enumerable.Range(0, 20000).Select(index => MediaFileFactory.Create($"Media {index}", $"Artist {index % 500}")).ToList();
        await _storeMetadata.ReplaceMediaFileList(mediaList);

        Stopwatch stopwatch = Stopwatch.StartNew();
        List<QueueItem> off = Have(await _service.Build(Library(), ShuffleModeEnum.Off, mediaList[0].ID));
        List<QueueItem> random = Have(await _service.Build(Library(), ShuffleModeEnum.Random, mediaList[0].ID));
        List<QueueItem> spread = Have(await _service.Build(Library(), ShuffleModeEnum.SpreadArtists, mediaList[0].ID));
        stopwatch.Stop();

        Assert.Equal(PresentationConstants.QueueMaxItemCount, random.Count);
        Assert.Equal(PresentationConstants.QueueMaxItemCount, spread.Count);
        Assert.NotEmpty(off);
        Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(10), $"Took {stopwatch.Elapsed}.");
    }
}
