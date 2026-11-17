using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Models;
using Noxy.NET.MediaPlayer.Domain.Responses.Remote;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;
using Noxy.NET.MediaPlayer.Presentation.Models;

namespace Noxy.NET.Test.MediaPlayer.Fixtures;

public abstract class LocalQueueStoreContract
{
    protected abstract ILocalQueueStore CreateStore();

    private static List<QueueItem> ItemList(int count)
    {
        return Enumerable.Range(0, count).Select(_ => new QueueItem(Guid.NewGuid(), Guid.NewGuid())).ToList();
    }

    private static LocalQueueSnapshot Snapshot(int historyCount, bool hasCurrent, int userCount, int sourceCount, bool hasSource, long revision = 7)
    {
        return new LocalQueueSnapshot(new ResponseQueue
        {
            QueueRevision = revision,
            Source = hasSource ? new ResponseQueueSource { Kind = QueueSourceKindEnum.Library, PlaylistID = null, Sort = LibrarySortEnum.Title, Descending = true, Search = "abc" } : null,
            HistoryList = ItemList(historyCount),
            Current = hasCurrent ? new QueueItem(Guid.NewGuid(), Guid.NewGuid()) : null,
            UserQueueList = ItemList(userCount),
            SourceList = ItemList(sourceCount)
        }, 12.5, new DateTime(2026, 11, 17, 10, 0, 0, DateTimeKind.Utc));
    }

    private static void AssertSame(LocalQueueSnapshot expected, LocalQueueSnapshot? actual)
    {
        Assert.NotNull(actual);
        Assert.Equal(expected.PositionSeconds, actual.PositionSeconds);
        Assert.Equal(expected.SavedAt, actual.SavedAt);
        Assert.Equal(expected.Queue.QueueRevision, actual.Queue.QueueRevision);
        Assert.Equal(expected.Queue.Current, actual.Queue.Current);
        Assert.Equal(expected.Queue.HistoryList, actual.Queue.HistoryList);
        Assert.Equal(expected.Queue.UserQueueList, actual.Queue.UserQueueList);
        Assert.Equal(expected.Queue.SourceList, actual.Queue.SourceList);
        Assert.Equal(expected.Queue.Source is null, actual.Queue.Source is null);
        if (expected.Queue.Source is null || actual.Queue.Source is null) return;

        Assert.Equal(expected.Queue.Source.Kind, actual.Queue.Source.Kind);
        Assert.Equal(expected.Queue.Source.PlaylistID, actual.Queue.Source.PlaylistID);
        Assert.Equal(expected.Queue.Source.Sort, actual.Queue.Source.Sort);
        Assert.Equal(expected.Queue.Source.Descending, actual.Queue.Source.Descending);
        Assert.Equal(expected.Queue.Source.Search, actual.Queue.Source.Search);
    }

    [Fact]
    public async Task Get_IsNull_BeforeAnyWrite()
    {
        Assert.Null(await CreateStore().Get());
    }

    [Theory]
    [InlineData(0, false, 0, 0, false)]
    [InlineData(3, true, 2, 5, true)]
    public async Task ASnapshot_RoundTripsEveryField(int historyCount, bool hasCurrent, int userCount, int sourceCount, bool hasSource)
    {
        ILocalQueueStore store = CreateStore();
        LocalQueueSnapshot snapshot = Snapshot(historyCount, hasCurrent, userCount, sourceCount, hasSource);

        await store.Save(snapshot);

        AssertSame(snapshot, await store.Get());
    }

    [Fact]
    public async Task Save_Replaces()
    {
        ILocalQueueStore store = CreateStore();
        LocalQueueSnapshot second = Snapshot(1, true, 1, 1, true, 9);

        await store.Save(Snapshot(2, true, 2, 2, false));
        await store.Save(second);

        AssertSame(second, await store.Get());
    }

    [Fact]
    public async Task Clear_Removes_AndIsHarmlessWhenEmpty()
    {
        ILocalQueueStore store = CreateStore();

        await store.Clear();
        await store.Save(Snapshot(1, true, 1, 1, true));
        await store.Clear();

        Assert.Null(await store.Get());
    }

    [Fact]
    public async Task AQueueOf20000Items_RoundTrips()
    {
        ILocalQueueStore store = CreateStore();
        LocalQueueSnapshot snapshot = Snapshot(0, true, 0, 20000, true);

        await store.Save(snapshot);

        AssertSame(snapshot, await store.Get());
    }
}
