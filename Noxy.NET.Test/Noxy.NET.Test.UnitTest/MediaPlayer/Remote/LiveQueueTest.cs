using System.ComponentModel;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Models;
using Noxy.NET.MediaPlayer.Domain.Responses.Remote;

namespace Noxy.NET.Test.MediaPlayer.Remote;

public class LiveQueueTest
{
    private const int Limit = 50;

    private readonly Dictionary<string, QueueItem> _itemMap = [];

    private static readonly ResponseQueueSource Source = new() { Kind = QueueSourceKindEnum.Library, PlaylistID = null, Sort = LibrarySortEnum.Title, Descending = false, Search = string.Empty };

    private QueueItem I(string name)
    {
        if (!_itemMap.TryGetValue(name, out QueueItem? item))
        {
            item = new(Guid.CreateVersion7(), Guid.CreateVersion7());
            _itemMap[name] = item;
        }

        return item;
    }

    private List<QueueItem> L(string names)
    {
        return [.. names.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(I)];
    }

    // history | current | user queue | source part, each part written as names separated by spaces.
    private LiveQueue Queue(string history = "", string current = "", string user = "", string source = "")
    {
        return LiveQueue.From(new()
        {
            QueueRevision = 0,
            Source = Source,
            HistoryList = L(history),
            Current = current.Length == 0 ? null : I(current),
            UserQueueList = L(user),
            SourceList = L(source)
        }, Limit);
    }

    private string Name(QueueItem? item)
    {
        return item is null ? string.Empty : _itemMap.Single(x => x.Value.ItemID == item.ItemID).Key;
    }

    private string Layout(LiveQueue queue)
    {
        ResponseQueue response = queue.ToResponse();
        return $"{string.Join(' ', response.HistoryList.Select(Name))}|{Name(response.Current)}|{string.Join(' ', response.UserQueueList.Select(Name))}|{string.Join(' ', response.SourceList.Select(Name))}";
    }

    private QueueEdit PlayNext(string name) => new() { Edit = QueueEditEnum.PlayNext, ItemID = I(name).ItemID, MediaFileID = I(name).MediaFileID };

    private QueueEdit AddToQueue(string name) => new() { Edit = QueueEditEnum.AddToQueue, ItemID = I(name).ItemID, MediaFileID = I(name).MediaFileID };

    private QueueEdit Remove(string name) => new() { Edit = QueueEditEnum.Remove, ItemID = I(name).ItemID };

    private QueueEdit Move(string name, string? after = null) => new() { Edit = QueueEditEnum.Move, ItemID = I(name).ItemID, AfterItemID = after is null ? null : I(after).ItemID };

    private QueueEdit Advance(string? name, bool keepSkipped = false) => new() { Edit = QueueEditEnum.Advance, Item = name is null ? null : I(name), KeepSkipped = keepSkipped };

    private QueueEdit Previous(string name, bool keepSkipped = false) => new() { Edit = QueueEditEnum.Previous, Item = I(name), KeepSkipped = keepSkipped };

    private QueueEdit ReplaceSource(string list, string current = "", bool keepUserQueue = false) => new()
    {
        Edit = QueueEditEnum.ReplaceSource,
        Source = Source,
        ItemList = [.. L(list)],
        CurrentItemID = current.Length == 0 ? null : I(current).ItemID,
        KeepUserQueue = keepUserQueue
    };

    private ResponseQueueDelta? Apply(LiveQueue queue, QueueEdit edit)
    {
        return queue.Apply(edit, Limit);
    }

    #region -- User queue --

    [Fact]
    public void PlayNext_PutsTheItemAtTheFrontOfTheUserQueue()
    {
        LiveQueue queue = Queue(current: "c", user: "u1", source: "s1");

        Apply(queue, PlayNext("n"));

        Assert.Equal("|c|n u1|s1", Layout(queue));
    }

    [Fact]
    public void AddToQueue_AppendsToTheUserQueue()
    {
        LiveQueue queue = Queue(current: "c", user: "u1", source: "s1");

        Apply(queue, AddToQueue("n"));

        Assert.Equal("|c|u1 n|s1", Layout(queue));
    }

    [Fact]
    public void PlayNext_ThenAddToQueue_KeepsPlayNextFirst()
    {
        LiveQueue queue = Queue(current: "c", source: "s1");

        Apply(queue, AddToQueue("a"));
        Apply(queue, PlayNext("p"));
        Apply(queue, AddToQueue("b"));

        Assert.Equal("|c|p a b|s1", Layout(queue));
    }

    [Fact]
    public void PlayNext_WithAnItemIDThatIsAlreadyInTheQueue_Throws_AndChangesNothing()
    {
        LiveQueue queue = Queue(current: "c", source: "s1");

        Assert.Throws<InvalidQueueEditException>(() => Apply(queue, PlayNext("s1")));

        Assert.Equal("|c||s1", Layout(queue));
        Assert.Equal(0, queue.QueueRevision);
    }

    [Theory]
    [InlineData("h1", "|c|u1|s1 s2")]
    [InlineData("c", "h1||u1|s1 s2")]
    [InlineData("u1", "h1|c||s1 s2")]
    [InlineData("s2", "h1|c|u1|s1")]
    public void Remove_FromEachPart_RemovesIt(string name, string expected)
    {
        LiveQueue queue = Queue(history: "h1", current: "c", user: "u1", source: "s1 s2");

        ResponseQueueDelta? delta = Apply(queue, Remove(name));

        Assert.NotNull(delta);
        Assert.Equal(expected, Layout(queue));
    }

    [Fact]
    public void Remove_TheCurrentItem_LeavesCurrentNull_AndOutOfHistory()
    {
        LiveQueue queue = Queue(history: "h1", current: "c", user: "u1", source: "s1");

        Apply(queue, Remove("c"));

        Assert.Equal("h1||u1|s1", Layout(queue));
    }

    [Fact]
    public void Remove_OfAGoneItem_IsANoOp_AndTheRevisionDoesNotRise()
    {
        LiveQueue queue = Queue(current: "c", user: "u1");

        ResponseQueueDelta? delta = Apply(queue, Remove("gone"));

        Assert.Null(delta);
        Assert.Equal(0, queue.QueueRevision);
        Assert.Equal("|c|u1|", Layout(queue));
    }

    [Fact]
    public void Move_InsideTheUpcomingPart_Reorders()
    {
        LiveQueue queue = Queue(current: "c", user: "u1 u2 u3", source: "s1 s2");

        Apply(queue, Move("u3", "u1"));
        Apply(queue, Move("s2", "s1"));

        Assert.Equal("|c|u1 u3 u2|s1 s2", Layout(queue));
        Apply(queue, Move("s1", "s2"));
        Assert.Equal("|c|u1 u3 u2|s2 s1", Layout(queue));
    }

    [Fact]
    public void Move_ASourceItemIntoTheUserQueue_MakesItAUserQueueItem()
    {
        LiveQueue queue = Queue(current: "c", user: "u1 u2", source: "s1 s2");

        Apply(queue, Move("s2", "u1"));

        Assert.Equal("|c|u1 s2 u2|s1", Layout(queue));
    }

    [Fact]
    public void Move_AUserQueueItemIntoTheSourcePart_MakesItASourceItem()
    {
        LiveQueue queue = Queue(current: "c", user: "u1 u2", source: "s1 s2");

        Apply(queue, Move("u1", "s1"));

        Assert.Equal("|c|u2|s1 u1 s2", Layout(queue));
    }

    [Fact]
    public void Move_WithANullAfter_GoesToTheFrontOfTheUserQueue()
    {
        LiveQueue queue = Queue(current: "c", user: "u1", source: "s1 s2");

        Apply(queue, Move("s2"));

        Assert.Equal("|c|s2 u1|s1", Layout(queue));
    }

    [Fact]
    public void Move_OfAGoneItemOrAGoneAnchor_IsANoOp()
    {
        LiveQueue queue = Queue(current: "c", user: "u1 u2");

        Assert.Null(Apply(queue, Move("gone", "u1")));
        Assert.Null(Apply(queue, Move("u1", "gone")));
        Assert.Equal("|c|u1 u2|", Layout(queue));
        Assert.Equal(0, queue.QueueRevision);
    }

    [Fact]
    public void Move_ToWhereItAlreadyIs_OrOntoItself_IsANoOp()
    {
        LiveQueue queue = Queue(current: "c", user: "u1 u2");

        Assert.Null(Apply(queue, Move("u1")));
        Assert.Null(Apply(queue, Move("u2", "u1")));
        Assert.Null(Apply(queue, Move("u2", "u2")));
        Assert.Equal(0, queue.QueueRevision);
    }

    #endregion -- User queue --

    #region -- Source --

    [Fact]
    public void ReplaceSource_SetsTheSourceCurrentAndClearsHistory()
    {
        LiveQueue queue = Queue(history: "h1", current: "c", user: "u1", source: "s1");

        Apply(queue, ReplaceSource("a b c2 d", "b"));

        Assert.Equal("|b||c2 d", Layout(queue));
        Assert.Equal(Source, queue.ToResponse().Source);
    }

    [Fact]
    public void ReplaceSource_WithKeepUserQueue_KeepsTheUserQueue_AndWithout_ClearsIt()
    {
        LiveQueue kept = Queue(current: "c", user: "u1 u2", source: "s1");
        LiveQueue cleared = Queue(current: "c", user: "u1 u2", source: "s1");

        Apply(kept, ReplaceSource("a b", "a", keepUserQueue: true));
        Apply(cleared, ReplaceSource("a b", "a", keepUserQueue: false));

        Assert.Equal("|a|u1 u2|b", Layout(kept));
        Assert.Equal("|a||b", Layout(cleared));
    }

    [Fact]
    public void ReplaceSource_WithNoCurrentItem_PutsTheWholeListInTheSourcePart()
    {
        LiveQueue queue = Queue(current: "c");

        Apply(queue, ReplaceSource("a b"));

        Assert.Equal("|||a b", Layout(queue));
    }

    [Fact]
    public void ReplaceSource_WithACurrentItemThatIsNotInTheList_Throws_AndChangesNothing()
    {
        LiveQueue queue = Queue(current: "c", source: "s1");
        QueueEdit edit = new() { Edit = QueueEditEnum.ReplaceSource, Source = Source, ItemList = [.. L("a b")], CurrentItemID = Guid.CreateVersion7() };

        Assert.Throws<InvalidQueueEditException>(() => Apply(queue, edit));

        Assert.Equal("|c||s1", Layout(queue));
    }

    [Fact]
    public void ReplaceSource_WithAKeptUserQueueThatSharesAnItemID_Throws()
    {
        LiveQueue queue = Queue(current: "c", user: "u1");

        Assert.Throws<InvalidQueueEditException>(() => Apply(queue, ReplaceSource("a u1", "a", keepUserQueue: true)));
    }

    [Fact]
    public void RegenerateSource_KeepsHistoryCurrentAndUserQueue()
    {
        LiveQueue queue = Queue(history: "h1", current: "c", user: "u1", source: "s1 s2");

        Apply(queue, new() { Edit = QueueEditEnum.RegenerateSource, ItemList = [.. L("s2 s1 s3")] });

        Assert.Equal("h1|c|u1|s2 s1 s3", Layout(queue));
    }

    [Fact]
    public void RegenerateSource_WithAnItemIDOfTheCurrentItem_Throws()
    {
        LiveQueue queue = Queue(current: "c", source: "s1");

        Assert.Throws<InvalidQueueEditException>(() => Apply(queue, new() { Edit = QueueEditEnum.RegenerateSource, ItemList = [.. L("c s1")] }));
    }

    [Fact]
    public void InsertSourceItem_AfterAnAnchor_AndWithAGoneAnchor_GoesToTheFront()
    {
        LiveQueue queue = Queue(current: "c", source: "s1 s2");

        Apply(queue, new() { Edit = QueueEditEnum.InsertSourceItem, Item = I("n1"), AfterItemID = I("s1").ItemID });
        Apply(queue, new() { Edit = QueueEditEnum.InsertSourceItem, Item = I("n2"), AfterItemID = Guid.CreateVersion7() });

        Assert.Equal("|c||n2 s1 n1 s2", Layout(queue));
    }

    [Fact]
    public void InsertSourceItem_WithADuplicateItemID_Throws()
    {
        LiveQueue queue = Queue(current: "c", source: "s1");

        Assert.Throws<InvalidQueueEditException>(() => Apply(queue, new() { Edit = QueueEditEnum.InsertSourceItem, Item = I("s1") }));
        Assert.Throws<InvalidQueueEditException>(() => Apply(queue, new() { Edit = QueueEditEnum.InsertSourceItem, Item = I("c") }));
    }

    [Fact]
    public void RemoveSourceItem_RemovesItFromTheSourcePart_AndLeavesTheCurrentItemAlone()
    {
        LiveQueue queue = Queue(current: "c", user: "u1", source: "s1 s2");

        Apply(queue, new() { Edit = QueueEditEnum.RemoveSourceItem, ItemID = I("s1").ItemID });

        Assert.Equal("|c|u1|s2", Layout(queue));
        Assert.Null(Apply(queue, new() { Edit = QueueEditEnum.RemoveSourceItem, ItemID = I("c").ItemID }));
        Assert.Null(Apply(queue, new() { Edit = QueueEditEnum.RemoveSourceItem, ItemID = I("u1").ItemID }));
        Assert.Equal("|c|u1|s2", Layout(queue));
    }

    #endregion -- Source --

    #region -- Advance --

    [Theory]
    [InlineData("u1", "c|u1|u2|s1 s2")]
    [InlineData("s1", "c|s1|u1 u2|s2")]
    public void Advance_MovesCurrentToHistory_AndMakesTheNamedItemCurrent_FromTheUserQueueOrTheSource(string name, string expected)
    {
        LiveQueue queue = Queue(current: "c", user: "u1 u2", source: "s1 s2");

        Apply(queue, Advance(name));

        Assert.Equal(expected, Layout(queue));
    }

    [Fact]
    public void Advance_NamingAnItemRemovedInARace_InsertsItAsCurrent_AndNeverErrors()
    {
        LiveQueue queue = Queue(current: "c", user: "u1", source: "s1");

        Apply(queue, Remove("u1"));
        ResponseQueueDelta? delta = Apply(queue, Advance("u1"));

        Assert.NotNull(delta);
        Assert.Equal("c|u1||s1", Layout(queue));
    }

    [Fact]
    public void RemoveThenAdvance_InsertsTheNamedItemAsCurrent_AndAdvanceThenRemove_SkipsIt_NeitherErrors()
    {
        LiveQueue removeFirst = Queue(current: "c", user: "u1 u2", source: "s1");
        LiveQueue advanceFirst = Queue(current: "c", user: "u1 u2", source: "s1");

        Apply(removeFirst, Remove("u1"));
        Apply(removeFirst, Advance("u1"));
        Apply(advanceFirst, Advance("u1"));
        Apply(advanceFirst, Remove("u1"));

        Assert.Equal("c|u1|u2|s1", Layout(removeFirst));
        Assert.Equal("c||u2|s1", Layout(advanceFirst));
    }

    [Fact]
    public void Advance_AfterTheCurrentWasRemoved_DoesNotPutItInHistory()
    {
        LiveQueue queue = Queue(history: "h1", current: "c", user: "u1", source: "s1");

        Apply(queue, Remove("c"));
        Apply(queue, Advance("u1"));

        Assert.Equal("h1|u1||s1", Layout(queue));
    }

    [Fact]
    public void Advance_ToAnItemFurtherDownTheSourcePart_DropsTheSkippedItems_NotIntoHistory()
    {
        LiveQueue queue = Queue(current: "c", source: "s1 s2 s3 s4");

        Apply(queue, Advance("s3"));

        Assert.Equal("c|s3||s4", Layout(queue));
    }

    [Fact]
    public void Advance_WithKeepSkipped_MovesTheSkippedItemsToHistory_InListOrder_AfterTheOldCurrent()
    {
        LiveQueue queue = Queue(history: "h1", current: "c", source: "s1 s2 s3 s4");

        Apply(queue, Advance("s3", keepSkipped: true));

        Assert.Equal("h1 c s1 s2|s3||s4", Layout(queue));
    }

    [Fact]
    public void Advance_ToAnItemFurtherDownTheUserQueue_DropsTheEarlierUserQueueItems()
    {
        LiveQueue queue = Queue(current: "c", user: "u1 u2 u3", source: "s1");

        Apply(queue, Advance("u2"));

        Assert.Equal("c|u2|u3|s1", Layout(queue));
    }

    [Fact]
    public void Advance_ToASourceItem_WhileTheUserQueueIsNotEmpty_LeavesTheUserQueueUntouched()
    {
        LiveQueue queue = Queue(current: "c", user: "u1 u2", source: "s1 s2");

        Apply(queue, Advance("s1"));

        Assert.Equal("c|s1|u1 u2|s2", Layout(queue));
    }

    [Fact]
    public void Advance_WithANullItem_LeavesCurrentNull_AndTheOldCurrentGoesToHistory()
    {
        LiveQueue queue = Queue(history: "h1", current: "c");

        Apply(queue, Advance(null));

        Assert.Equal("h1 c|||", Layout(queue));
        Assert.Null(Apply(queue, Advance(null)));
    }

    [Fact]
    public void Advance_NamingTheCurrentItem_IsANoOp()
    {
        LiveQueue queue = Queue(current: "c", source: "s1");

        Assert.Null(Apply(queue, Advance("c")));
        Assert.Equal(0, queue.QueueRevision);
    }

    [Fact]
    public void Advance_TrimsHistoryToTheLimit()
    {
        LiveQueue queue = Queue(current: "i0", source: string.Join(' ', Enumerable.Range(1, Limit + 5).Select(x => $"i{x}")));

        for (int index = 1; index <= Limit + 5; index++)
        {
            Apply(queue, Advance($"i{index}"));
        }

        ResponseQueue response = queue.ToResponse();
        Assert.Equal(Limit, response.HistoryList.Count);
        Assert.Equal("i54", Name(response.HistoryList[^1]));
        Assert.Equal("i55", Name(response.Current));
    }

    [Fact]
    public void History_HoldsOnlyHeardOrNextedSongs()
    {
        LiveQueue queue = Queue(current: "c", user: "u1", source: "s1 s2 s3");

        Apply(queue, Advance("s2"));
        Apply(queue, Remove("s3"));
        Apply(queue, Advance("u1"));

        Assert.Equal("c s2|u1||", Layout(queue));
    }

    #endregion -- Advance --

    #region -- Previous --

    [Fact]
    public void Previous_MakesTheNamedHistoryItemCurrent_AndPutsCurrentBackAtTheFrontOfTheSourcePart()
    {
        LiveQueue queue = Queue(history: "h1 h2", current: "c", user: "u1", source: "s1");

        Apply(queue, Previous("h2"));

        Assert.Equal("h1|h2|u1|c s1", Layout(queue));
    }

    [Fact]
    public void Previous_ThenAdvance_PlaysTheUserQueueBeforeTheItemThatWentBack()
    {
        LiveQueue queue = Queue(history: "h1", current: "c", user: "u1", source: "s1");

        Apply(queue, Previous("h1"));
        Apply(queue, Advance("u1"));

        Assert.Equal("h1|u1||c s1", Layout(queue));
    }

    [Fact]
    public void Previous_FurtherBack_DropsTheItemsBetween_ByDefault()
    {
        LiveQueue queue = Queue(history: "h1 h2 h3", current: "c", source: "s1");

        Apply(queue, Previous("h1"));

        Assert.Equal("|h1||c s1", Layout(queue));
    }

    [Fact]
    public void Previous_FurtherBack_WithKeepSkipped_PutsThemAtTheFrontOfTheSourcePart_InOrder()
    {
        LiveQueue queue = Queue(history: "h1 h2 h3", current: "c", source: "s1");

        Apply(queue, Previous("h1", keepSkipped: true));

        Assert.Equal("|h1||h2 h3 c s1", Layout(queue));
    }

    [Fact]
    public void PreviousSeveralTimes_PushesEachSongSteppedBackOverToTheFrontOfTheSourcePart_InOrder()
    {
        LiveQueue queue = Queue(history: "h1 h2", current: "c", source: "s1");

        Apply(queue, Previous("h2"));
        Apply(queue, Previous("h1"));

        Assert.Equal("|h1||h2 c s1", Layout(queue));
    }

    [Fact]
    public void Advance_ThenPrevious_AfterAJump_ReturnsToTheSongThatWasHeard()
    {
        LiveQueue queue = Queue(current: "c", source: "s1 s2 s3");

        Apply(queue, Advance("s3"));
        Apply(queue, Previous("c"));

        Assert.Equal("|c||s3", Layout(queue));
    }

    [Fact]
    public void Previous_OverAUserQueueItem_MakesItASourcePartItem_SoANewSourceDoesNotKeepIt()
    {
        LiveQueue queue = Queue(history: "h1", current: "u1", source: "s1");

        Apply(queue, Previous("h1"));
        Assert.Equal("|h1||u1 s1", Layout(queue));

        Apply(queue, ReplaceSource("a b", "a", keepUserQueue: true));
        Assert.Equal("|a||b", Layout(queue));
    }

    [Fact]
    public void Previous_NamingAnItemNotInHistory_InsertsItAsCurrent()
    {
        LiveQueue queue = Queue(current: "c", source: "s1");

        Apply(queue, Previous("x"));

        Assert.Equal("|x||c s1", Layout(queue));
    }

    [Fact]
    public void Previous_NamingAnItemThatIsUpcoming_MovesItToCurrent_WithoutDuplicatingIt()
    {
        LiveQueue queue = Queue(current: "c", user: "u1", source: "s1");

        Apply(queue, Previous("s1"));

        Assert.Equal("|s1|u1|c", Layout(queue));
    }

    [Fact]
    public void Previous_NamingTheCurrentItem_IsANoOp()
    {
        LiveQueue queue = Queue(history: "h1", current: "c");

        Assert.Null(Apply(queue, Previous("c")));
    }

    #endregion -- Previous --

    #region -- Revision and shape --

    [Fact]
    public void EveryAppliedEdit_RaisesTheRevisionByOne()
    {
        LiveQueue queue = Queue(current: "c", source: "s1 s2");

        ResponseQueueDelta? first = Apply(queue, PlayNext("a"));
        ResponseQueueDelta? second = Apply(queue, AddToQueue("b"));
        Apply(queue, Remove("gone"));
        ResponseQueueDelta? third = Apply(queue, Advance("a"));

        Assert.Equal(new long?[] { 1, 2, 3 }, [first?.QueueRevision, second?.QueueRevision, third?.QueueRevision]);
        Assert.Equal(3, queue.QueueRevision);
    }

    [Fact]
    public void TheDelta_CarriesTheEditAndItsFields()
    {
        LiveQueue queue = Queue(current: "c", source: "s1");

        ResponseQueueDelta? delta = Apply(queue, Advance("s1", keepSkipped: true));

        Assert.NotNull(delta);
        Assert.Equal(QueueEditEnum.Advance, delta.Edit);
        Assert.Equal(I("s1"), delta.Item);
        Assert.True(delta.KeepSkipped);
    }

    [Fact]
    public void TwoEditsInArrivalOrder_GiveOneConsistentList()
    {
        LiveQueue queue = Queue(current: "c", user: "u1 u2", source: "s1");

        Apply(queue, Move("u2"));
        Apply(queue, Remove("u2"));
        Apply(queue, Advance("u1"));

        Assert.Equal("c|u1||s1", Layout(queue));
        Assert.Equal(3, queue.QueueRevision);
    }

    [Fact]
    public void AppliedToTwoCopies_TheSameDeltasGiveTheSameQueue()
    {
        LiveQueue server = Queue(current: "c", user: "u1", source: "s1 s2 s3");
        LiveQueue client = LiveQueue.From(server.ToResponse(), Limit);
        List<QueueEdit> editList = [PlayNext("n"), Advance("s2", keepSkipped: true), Move("s3", "n"), Previous("s1")];

        foreach (QueueEdit edit in editList)
        {
            ResponseQueueDelta? delta = Apply(server, edit);
            if (delta is not null) Apply(client, edit);
        }

        Assert.Equal(Layout(server), Layout(client));
        Assert.Equal(server.QueueRevision, client.QueueRevision);
    }

    [Fact]
    public void From_WithDuplicateItemIDs_Throws()
    {
        QueueItem item = I("a");

        Assert.Throws<InvalidQueueEditException>(() => LiveQueue.From(new()
        {
            QueueRevision = 0,
            Source = null,
            HistoryList = [item],
            Current = null,
            UserQueueList = [],
            SourceList = [item]
        }, Limit));
    }

    [Fact]
    public void From_TrimsAHistoryLongerThanTheLimit()
    {
        ResponseQueue response = new()
        {
            QueueRevision = 4,
            Source = null,
            HistoryList = [.. Enumerable.Range(0, 10).Select(x => I($"h{x}"))],
            Current = null,
            UserQueueList = [],
            SourceList = []
        };

        LiveQueue queue = LiveQueue.From(response, 3);

        Assert.Equal(["h7", "h8", "h9"], queue.ToResponse().HistoryList.Select(Name));
        Assert.Equal(4, queue.QueueRevision);
    }

    [Fact]
    public void ToResponse_RoundTripsThroughFrom()
    {
        LiveQueue queue = Queue(history: "h1", current: "c", user: "u1", source: "s1 s2");
        Apply(queue, AddToQueue("u2"));

        LiveQueue copy = LiveQueue.From(queue.ToResponse(), Limit);

        Assert.Equal(Layout(queue), Layout(copy));
        Assert.Equal(queue.QueueRevision, copy.QueueRevision);
        Assert.Equal(queue.ToResponse().Source, copy.ToResponse().Source);
    }

    [Fact]
    public void ToResponse_ReturnsCopies_SoLaterEditsDoNotChangeAnEarlierSnapshot()
    {
        LiveQueue queue = Queue(current: "c", user: "u1");
        ResponseQueue snapshot = queue.ToResponse();

        Apply(queue, AddToQueue("u2"));

        Assert.Single(snapshot.UserQueueList);
    }

    [Fact]
    public void ItemCount_CountsEveryPart()
    {
        Assert.Equal(6, Queue(history: "h1", current: "c", user: "u1", source: "s1 s2 s3").ItemCount);
    }

    #endregion -- Revision and shape --

    [Fact]
    public void AnUndefinedEdit_ThrowsAnInvalidEnumException_AndChangesNothing()
    {
        LiveQueue queue = Queue(current: "c", user: "u1");

        Assert.Throws<InvalidEnumArgumentException>(() => Apply(queue, new() { Edit = (QueueEditEnum)999 }));

        Assert.Equal("|c|u1|", Layout(queue));
        Assert.Equal(0, queue.QueueRevision);
    }

    [Fact]
    public void ANullItemInAList_Throws_InsteadOfFailingWithANullReference()
    {
        QueueEdit edit = new() { Edit = QueueEditEnum.RegenerateSource, ItemList = [I("a"), null] };
        LiveQueue queue = Queue(current: "c");

        Assert.Throws<InvalidQueueEditException>(() => Apply(queue, edit));
        Assert.Throws<InvalidQueueEditException>(() => LiveQueue.From(new() { QueueRevision = 0, Source = null, HistoryList = [], Current = null, UserQueueList = [], SourceList = [null!] }, Limit));
    }

    [Fact]
    public void TheCountsOfTheUserQueueAndTheSourcePart_AreExposed()
    {
        LiveQueue queue = Queue(history: "h1", current: "c", user: "u1 u2", source: "s1 s2 s3");

        Assert.Equal(2, queue.UserQueueCount);
        Assert.Equal(3, queue.SourceCount);
    }
}
