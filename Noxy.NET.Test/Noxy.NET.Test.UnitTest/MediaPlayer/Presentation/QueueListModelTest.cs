using Noxy.NET.MediaPlayer.Domain.Models;
using Noxy.NET.MediaPlayer.Domain.Responses.Remote;
using Noxy.NET.MediaPlayer.Presentation.Enums;
using Noxy.NET.MediaPlayer.Presentation.Extensions;
using Noxy.NET.MediaPlayer.Presentation.Models;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class QueueListModelTest
{
    private static QueueItem Item()
    {
        return new(Guid.NewGuid(), Guid.NewGuid());
    }

    private static List<QueueItem> ItemList(int count)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Item())];
    }

    private static ResponseQueue Queue(List<QueueItem> history, QueueItem? current, List<QueueItem> userQueue, List<QueueItem> source)
    {
        return new() { QueueRevision = 1, Source = null, HistoryList = history, Current = current, UserQueueList = userQueue, SourceList = source };
    }

    [Fact]
    public void ToListModel_OrdersHistoryCurrentUserQueueThenSource_WithTheThreeDividers()
    {
        QueueListModel model = Queue(ItemList(2), Item(), ItemList(2), ItemList(2)).ToListModel(null);

        Assert.Equal(
            [
                QueueRowKindEnum.History, QueueRowKindEnum.History, QueueRowKindEnum.DividerCurrent, QueueRowKindEnum.Current, QueueRowKindEnum.DividerUserQueue, QueueRowKindEnum.UserQueue, QueueRowKindEnum.UserQueue,
                QueueRowKindEnum.DividerSource, QueueRowKindEnum.Source, QueueRowKindEnum.Source
            ],
            model.RowList.Select(x => x.Kind));
        Assert.Equal(3, model.CurrentIndex);
    }

    [Fact]
    public void ToListModel_KeepsTheItemsInTheOrderOfEachPart()
    {
        List<QueueItem> history = ItemList(2);
        List<QueueItem> userQueue = ItemList(2);
        List<QueueItem> source = ItemList(2);

        QueueListModel model = Queue(history, Item(), userQueue, source).ToListModel(null);

        Assert.Equal([.. history, .. userQueue, .. source], model.RowList.Where(x => x.Item is not null && x.Kind != QueueRowKindEnum.Current).Select(x => x.Item));
    }

    [Fact]
    public void ToListModel_LeavesOutTheDividerOfAnEmptyPart()
    {
        QueueListModel model = Queue([], Item(), [], []).ToListModel(null);

        Assert.Equal([QueueRowKindEnum.DividerCurrent, QueueRowKindEnum.Current], model.RowList.Select(x => x.Kind));
    }

    [Fact]
    public void ToListModel_WithNothingCurrent_HasNoCurrentDividerAndAnIndexOfMinusOne()
    {
        QueueListModel model = Queue(ItemList(1), null, [], ItemList(1)).ToListModel(null);

        Assert.DoesNotContain(model.RowList, x => x.Kind is QueueRowKindEnum.DividerCurrent or QueueRowKindEnum.Current);
        Assert.Equal(-1, model.CurrentIndex);
    }

    [Fact]
    public void ToListModel_OfAnEmptyQueue_HasNoRows()
    {
        QueueListModel model = Queue([], null, [], []).ToListModel(null);

        Assert.Empty(model.RowList);
        Assert.Equal(-1, model.CurrentIndex);
    }

    [Fact]
    public void ToListModel_KeepsASongQueuedTwiceAsTwoRows()
    {
        Guid mediaFileID = Guid.NewGuid();
        List<QueueItem> userQueue = [new(Guid.NewGuid(), mediaFileID), new(Guid.NewGuid(), mediaFileID)];

        QueueListModel model = Queue([], Item(), userQueue, []).ToListModel(null);

        List<QueueListRow> queuedList = [.. model.RowList.Where(x => x.Kind == QueueRowKindEnum.UserQueue)];
        Assert.Equal(2, queuedList.Count);
        Assert.NotEqual(queuedList[0].Item?.ItemID, queuedList[1].Item?.ItemID);
    }

    [Fact]
    public void ToListModel_SourceDividerCarriesTheSourceName_AndNoOtherRowDoes()
    {
        QueueListModel model = Queue(ItemList(1), Item(), ItemList(1), ItemList(1)).ToListModel("Road trip");

        Assert.Equal("Road trip", model.RowList.Single(x => x.Kind == QueueRowKindEnum.DividerSource).SourceName);
        Assert.All(model.RowList.Where(x => x.Kind != QueueRowKindEnum.DividerSource), x => Assert.Null(x.SourceName));
    }

    [Fact]
    public void ToListModel_DividersHaveNoItem_AndItemRowsHaveOne()
    {
        QueueListModel model = Queue(ItemList(1), Item(), ItemList(1), ItemList(1)).ToListModel(null);

        Assert.All(model.RowList, x => Assert.Equal(x.Kind.IsItem, x.Item is not null));
    }

    [Fact]
    public void ToListModel_Of5000SourceItems_HasOneRowPerItemPlusTheDividers()
    {
        QueueListModel model = Queue([], Item(), [], ItemList(5000)).ToListModel(null);

        Assert.Equal(5000 + 3, model.RowList.Count);
        Assert.Equal(1, model.CurrentIndex);
    }

    [Theory]
    [InlineData(QueueRowKindEnum.DividerCurrent, false, false)]
    [InlineData(QueueRowKindEnum.DividerUserQueue, false, false)]
    [InlineData(QueueRowKindEnum.DividerSource, false, false)]
    [InlineData(QueueRowKindEnum.History, true, false)]
    [InlineData(QueueRowKindEnum.Current, true, false)]
    [InlineData(QueueRowKindEnum.UserQueue, true, true)]
    [InlineData(QueueRowKindEnum.Source, true, true)]
    public void IsItemAndIsUpcoming_AreRightForEveryKind(QueueRowKindEnum kind, bool isItem, bool isUpcoming)
    {
        Assert.Equal(isItem, kind.IsItem);
        Assert.Equal(isUpcoming, kind.IsUpcoming);
    }

    [Fact]
    public void EveryKindIsCoveredByTheTheoryAbove()
    {
        Assert.Equal(7, Enum.GetValues<QueueRowKindEnum>().Length);
    }
}
