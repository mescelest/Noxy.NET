using System.ComponentModel;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Models;
using Noxy.NET.MediaPlayer.Presentation.Services;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class ShuffleServiceTest
{
    private static List<QueueItem> ItemList(int count)
    {
        return Enumerable.Range(0, count).Select(_ => new QueueItem(Guid.NewGuid(), Guid.NewGuid())).ToList();
    }

    private static string? NoArtist(Guid mediaFileID)
    {
        return null;
    }

    [Fact]
    public void Off_ReturnsTheOrder_AsACopy()
    {
        List<QueueItem> itemList = ItemList(5);

        List<QueueItem> result = new ShuffleService(new Random(1)).Order(itemList, ShuffleModeEnum.Off, NoArtist);

        Assert.Equal(itemList, result);
        Assert.NotSame(itemList, result);
    }

    [Fact]
    public void Random_KeepsEveryItemExactlyOnce()
    {
        List<QueueItem> itemList = ItemList(200);

        List<QueueItem> result = new ShuffleService(new Random(2)).Order(itemList, ShuffleModeEnum.Random, NoArtist);

        Assert.Equal(itemList.Count, result.Count);
        Assert.Equal(itemList.OrderBy(x => x.ItemID), result.OrderBy(x => x.ItemID));
        Assert.NotEqual(itemList, result);
    }

    [Fact]
    public void Random_WithAFixedRandom_IsReproducible()
    {
        List<QueueItem> itemList = ItemList(50);

        List<QueueItem> first = new ShuffleService(new Random(3)).Order(itemList, ShuffleModeEnum.Random, NoArtist);
        List<QueueItem> second = new ShuffleService(new Random(3)).Order(itemList, ShuffleModeEnum.Random, NoArtist);

        Assert.Equal(first, second);
    }

    [Fact]
    public void Random_DuplicateMediaFilesInAPlaylist_AreShuffledAsSeparateItems()
    {
        Guid mediaFileID = Guid.NewGuid();
        List<QueueItem> itemList = [new(Guid.NewGuid(), mediaFileID), new(Guid.NewGuid(), mediaFileID), new(Guid.NewGuid(), mediaFileID)];

        List<QueueItem> result = new ShuffleService(new Random(4)).Order(itemList, ShuffleModeEnum.Random, NoArtist);

        Assert.Equal(3, result.Select(x => x.ItemID).Distinct().Count());
    }

    [Fact]
    public void Random_Distribution_FirstPositionIsRoughlyUniform_AcrossManyRuns()
    {
        List<QueueItem> itemList = ItemList(5);
        ShuffleService service = new(new Random(5));
        Dictionary<Guid, int> firstCount = itemList.ToDictionary(x => x.ItemID, _ => 0);

        for (int run = 0; run < 1000; run++) firstCount[service.Order(itemList, ShuffleModeEnum.Random, NoArtist)[0].ItemID]++;

        Assert.All(firstCount.Values, count => Assert.InRange(count, 150, 250));
    }

    [Fact]
    public void SpreadArtists_NeverPlacesTwoMediaFilesOfOneArtistTogether_WhenThatIsPossible()
    {
        List<QueueItem> itemList = ItemList(30);
        Dictionary<Guid, string> artistMap = itemList.Select((item, index) => (item, index)).ToDictionary(x => x.item.MediaFileID, x => $"Artist {x.index % 10}");

        List<QueueItem> result = new ShuffleService(new Random(6)).Order(itemList, ShuffleModeEnum.SpreadArtists, id => artistMap[id]);

        for (int index = 1; index < result.Count; index++) Assert.NotEqual(artistMap[result[index - 1].MediaFileID], artistMap[result[index].MediaFileID]);
    }

    [Fact]
    public void SpreadArtists_WhenOneArtistHasMoreThanHalfTheItems_StillKeepsEveryItemOnce()
    {
        List<QueueItem> itemList = ItemList(20);
        HashSet<Guid> dominant = itemList.Take(15).Select(x => x.MediaFileID).ToHashSet();

        List<QueueItem> result = new ShuffleService(new Random(7)).Order(itemList, ShuffleModeEnum.SpreadArtists, id => dominant.Contains(id) ? "Same" : id.ToString());

        Assert.Equal(itemList.OrderBy(x => x.ItemID), result.OrderBy(x => x.ItemID));
    }

    [Fact]
    public void SpreadArtists_IgnoresArtistCase_AndNeverTreatsMissingArtistsAsEqual()
    {
        List<QueueItem> itemList = ItemList(20);
        string[] spellingList = ["Abba", "ABBA", "abba"];
        Dictionary<Guid, string> artistMap = itemList.Select((item, index) => (item, index)).ToDictionary(x => x.item.MediaFileID, x => x.index < 6 ? spellingList[x.index % 3] : $"Artist {x.index}");

        List<QueueItem> result = new ShuffleService(new Random(9)).Order(itemList, ShuffleModeEnum.SpreadArtists, id => artistMap[id]);

        for (int index = 1; index < result.Count; index++) Assert.False(string.Equals(artistMap[result[index - 1].MediaFileID], artistMap[result[index].MediaFileID], StringComparison.OrdinalIgnoreCase));

        List<QueueItem> missingList = ItemList(4);
        Assert.Equal(missingList.OrderBy(x => x.ItemID), new ShuffleService(new Random(8)).Order(missingList, ShuffleModeEnum.SpreadArtists, NoArtist).OrderBy(x => x.ItemID));
    }

    [Fact]
    public void AnUnknownMode_Throws()
    {
        Assert.Throws<InvalidEnumArgumentException>(() => new ShuffleService(new Random(1)).Order(ItemList(2), (ShuffleModeEnum)99, NoArtist));
    }
}
