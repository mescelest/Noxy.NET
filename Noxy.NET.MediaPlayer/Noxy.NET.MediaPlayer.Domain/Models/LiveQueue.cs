using System.ComponentModel;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Responses.Remote;

namespace Noxy.NET.MediaPlayer.Domain.Models;

public class LiveQueue
{
    private ResponseQueueSource? _source;
    private List<QueueItem> _historyList = [];
    private QueueItem? _current;
    private List<QueueItem> _userQueueList = [];
    private List<QueueItem> _sourceList = [];

    public long QueueRevision { get; set; }

    public int ItemCount => _historyList.Count + (_current is null ? 0 : 1) + _userQueueList.Count + _sourceList.Count;
    public int UserQueueCount => _userQueueList.Count;
    public int SourceCount => _sourceList.Count;

    public ResponseQueue ToResponse()
    {
        return new()
        {
            QueueRevision = QueueRevision,
            Source = _source,
            HistoryList = [.. _historyList],
            Current = _current,
            UserQueueList = [.. _userQueueList],
            SourceList = [.. _sourceList]
        };
    }

    // The queue a device sends when it claims to be the active one. An item ID may appear only once in the whole queue.
    public static LiveQueue From(ResponseQueue queue, int historyLimit)
    {
        List<QueueItem> allList = [.. queue.HistoryList, .. queue.UserQueueList, .. queue.SourceList];
        if (queue.Current is not null) allList.Add(queue.Current);

        EnsureDistinct(allList);

        List<QueueItem> historyList = [.. queue.HistoryList];
        if (historyList.Count > historyLimit) historyList.RemoveRange(0, historyList.Count - historyLimit);

        return new()
        {
            QueueRevision = queue.QueueRevision,
            _source = queue.Source,
            _historyList = historyList,
            _current = queue.Current,
            _userQueueList = [.. queue.UserQueueList],
            _sourceList = [.. queue.SourceList]
        };
    }

    // Applies one edit. Returns null when it changed nothing (an item that is gone is not an error); otherwise the revision rises by one and the pushed form of the edit is returned.
    // An edit that is not valid throws before anything is changed.
    public ResponseQueueDelta? Apply(QueueEdit edit, int historyLimit)
    {
        bool applied = edit.Edit switch
        {
            QueueEditEnum.PlayNext => PlayNext(edit),
            QueueEditEnum.AddToQueue => AddToQueue(edit),
            QueueEditEnum.Remove => Remove(edit),
            QueueEditEnum.Move => Move(edit),
            QueueEditEnum.ReplaceSource => ReplaceSource(edit),
            QueueEditEnum.RegenerateSource => RegenerateSource(edit),
            QueueEditEnum.InsertSourceItem => InsertSourceItem(edit),
            QueueEditEnum.RemoveSourceItem => RemoveSourceItem(edit),
            QueueEditEnum.Advance => Advance(edit, historyLimit),
            QueueEditEnum.Previous => Previous(edit),
            _ => throw new InvalidEnumArgumentException(nameof(edit), (int)edit.Edit, typeof(QueueEditEnum))
        };

        if (!applied) return null;

        QueueRevision++;
        return new()
        {
            QueueRevision = QueueRevision,
            Edit = edit.Edit,
            ItemID = edit.ItemID,
            MediaFileID = edit.MediaFileID,
            AfterItemID = edit.AfterItemID,
            Item = edit.Item,
            ItemList = edit.ItemList,
            Source = edit.Source,
            CurrentItemID = edit.CurrentItemID,
            KeepUserQueue = edit.KeepUserQueue,
            KeepSkipped = edit.KeepSkipped
        };
    }

    #region -- User queue --

    private bool PlayNext(QueueEdit edit)
    {
        QueueItem item = NewItem(edit);
        _userQueueList.Insert(0, item);
        return true;
    }

    private bool AddToQueue(QueueEdit edit)
    {
        QueueItem item = NewItem(edit);
        _userQueueList.Add(item);
        return true;
    }

    // Removing the playing item means "skip": it leaves and does not enter history. Any other item just leaves.
    private bool Remove(QueueEdit edit)
    {
        Guid itemID = Require(edit.ItemID, nameof(edit.ItemID));
        if (_current?.ItemID == itemID)
        {
            _current = null;
            return true;
        }

        return _userQueueList.RemoveAll(x => x.ItemID == itemID) + _sourceList.RemoveAll(x => x.ItemID == itemID) + _historyList.RemoveAll(x => x.ItemID == itemID) > 0;
    }

    // Reorders inside the upcoming part. The item takes its place after the anchor, in whichever list the anchor is in; no anchor, or the current item as the anchor, means the front of the user queue.
    private bool Move(QueueEdit edit)
    {
        Guid itemID = Require(edit.ItemID, nameof(edit.ItemID));
        QueueItem? item = _userQueueList.Find(x => x.ItemID == itemID) ?? _sourceList.Find(x => x.ItemID == itemID);
        Guid? afterID = edit.AfterItemID;
        if (item is null || afterID == itemID) return false;

        bool toFront = afterID is null || afterID == _current?.ItemID;
        bool anchorInUserQueue = afterID is { } userAnchor && _userQueueList.Any(x => x.ItemID == userAnchor);
        bool anchorInSource = afterID is { } sourceAnchor && _sourceList.Any(x => x.ItemID == sourceAnchor);
        if (!toFront && !anchorInUserQueue && !anchorInSource) return false;

        List<QueueItem> userBefore = [.. _userQueueList];
        List<QueueItem> sourceBefore = [.. _sourceList];
        _userQueueList.RemoveAll(x => x.ItemID == itemID);
        _sourceList.RemoveAll(x => x.ItemID == itemID);

        if (toFront) _userQueueList.Insert(0, item);
        else if (anchorInUserQueue) _userQueueList.Insert(_userQueueList.FindIndex(x => x.ItemID == afterID) + 1, item);
        else _sourceList.Insert(_sourceList.FindIndex(x => x.ItemID == afterID) + 1, item);

        return !userBefore.SequenceEqual(_userQueueList) || !sourceBefore.SequenceEqual(_sourceList);
    }

    #endregion -- User queue --

    #region -- Source --

    // A new source: the list after the current item becomes the source part, history is cleared and the user queue is kept only when asked.
    private bool ReplaceSource(QueueEdit edit)
    {
        ResponseQueueSource source = edit.Source ?? throw new InvalidQueueEditException("A new source needs the source.");
        List<QueueItem> itemList = edit.ItemList ?? throw new InvalidQueueEditException("A new source needs the items.");
        EnsureDistinct(itemList);

        QueueItem? current = null;
        List<QueueItem> rest = itemList;
        if (edit.CurrentItemID is { } currentItemID)
        {
            int index = itemList.FindIndex(x => x.ItemID == currentItemID);
            if (index < 0) throw new InvalidQueueEditException("The current item is not in the new source.");

            current = itemList[index];
            rest = itemList.GetRange(index + 1, itemList.Count - index - 1);
        }

        List<QueueItem> userQueueList = edit.KeepUserQueue ? _userQueueList : [];
        EnsureDistinct([.. itemList, .. userQueueList]);

        _source = source;
        _historyList = [];
        _current = current;
        _userQueueList = [.. userQueueList];
        _sourceList = [.. rest];
        return true;
    }

    private bool RegenerateSource(QueueEdit edit)
    {
        List<QueueItem> itemList = edit.ItemList ?? throw new InvalidQueueEditException("A regenerated source needs the items.");
        List<QueueItem> others = [.. _historyList, .. _userQueueList];
        if (_current is not null) others.Add(_current);

        EnsureDistinct([.. itemList, .. others]);

        _sourceList = [.. itemList];
        return true;
    }

    private bool InsertSourceItem(QueueEdit edit)
    {
        QueueItem item = edit.Item ?? throw new InvalidQueueEditException("An inserted item needs the item.");
        EnsureNew(item.ItemID);

        int index = edit.AfterItemID is { } afterID ? _sourceList.FindIndex(x => x.ItemID == afterID) : -1;
        _sourceList.Insert(index + 1, item);
        return true;
    }

    // The source changed underneath: only the source part is touched, so a playing item is left alone to finish.
    private bool RemoveSourceItem(QueueEdit edit)
    {
        Guid itemID = Require(edit.ItemID, nameof(edit.ItemID));
        return _sourceList.RemoveAll(x => x.ItemID == itemID) > 0;
    }

    #endregion -- Source --

    #region -- Advance and previous --

    // The active device reports the item it is now playing. The old current item goes to history; the named item is taken from wherever it is, or inserted when it is nowhere.
    private bool Advance(QueueEdit edit, int historyLimit)
    {
        QueueItem? target = edit.Item;
        if (target is null)
        {
            if (_current is null) return false;

            PushHistory([_current], historyLimit);
            _current = null;
            return true;
        }

        if (_current?.ItemID == target.ItemID) return false;

        QueueItem named = target;
        List<QueueItem> skippedList = [];
        int userIndex = _userQueueList.FindIndex(x => x.ItemID == target.ItemID);
        int sourceIndex = _sourceList.FindIndex(x => x.ItemID == target.ItemID);
        if (userIndex >= 0)
        {
            named = _userQueueList[userIndex];
            skippedList = _userQueueList.GetRange(0, userIndex);
            _userQueueList.RemoveRange(0, userIndex + 1);
        }
        else if (sourceIndex >= 0)
        {
            named = _sourceList[sourceIndex];
            skippedList = _sourceList.GetRange(0, sourceIndex);
            _sourceList.RemoveRange(0, sourceIndex + 1);
        }
        else
        {
            _historyList.RemoveAll(x => x.ItemID == target.ItemID);
        }

        // Skipped items never enter history unless the user jumped on purpose.
        List<QueueItem> toHistoryList = [];
        if (_current is not null) toHistoryList.Add(_current);
        if (edit.KeepSkipped) toHistoryList.AddRange(skippedList);

        PushHistory(toHistoryList, historyLimit);
        _current = named;
        return true;
    }

    // One step back in the normal queue: the item left goes to the front of the source part, whatever part it came from.
    private bool Previous(QueueEdit edit)
    {
        QueueItem target = edit.Item ?? throw new InvalidQueueEditException("A step back needs the item.");
        if (_current?.ItemID == target.ItemID) return false;

        QueueItem named = target;
        List<QueueItem> afterList = [];
        int historyIndex = _historyList.FindIndex(x => x.ItemID == target.ItemID);
        if (historyIndex >= 0)
        {
            named = _historyList[historyIndex];
            afterList = _historyList.GetRange(historyIndex + 1, _historyList.Count - historyIndex - 1);
            _historyList.RemoveRange(historyIndex, _historyList.Count - historyIndex);
        }
        else
        {
            named = _userQueueList.Find(x => x.ItemID == target.ItemID) ?? _sourceList.Find(x => x.ItemID == target.ItemID) ?? target;
            _userQueueList.RemoveAll(x => x.ItemID == target.ItemID);
            _sourceList.RemoveAll(x => x.ItemID == target.ItemID);
        }

        List<QueueItem> frontList = [];
        if (edit.KeepSkipped) frontList.AddRange(afterList);
        if (_current is not null) frontList.Add(_current);

        _sourceList.InsertRange(0, frontList);
        _current = named;
        return true;
    }

    private void PushHistory(List<QueueItem> itemList, int historyLimit)
    {
        _historyList.AddRange(itemList);
        if (_historyList.Count > historyLimit) _historyList.RemoveRange(0, _historyList.Count - historyLimit);
    }

    #endregion -- Advance and previous --

    #region -- Checks --

    private QueueItem NewItem(QueueEdit edit)
    {
        QueueItem item = new(Require(edit.ItemID, nameof(edit.ItemID)), Require(edit.MediaFileID, nameof(edit.MediaFileID)));
        EnsureNew(item.ItemID);
        return item;
    }

    private void EnsureNew(Guid itemID)
    {
        if (_current?.ItemID == itemID || _historyList.Any(x => x.ItemID == itemID) || _userQueueList.Any(x => x.ItemID == itemID) || _sourceList.Any(x => x.ItemID == itemID))
        {
            throw new InvalidQueueEditException("The item ID is already in the queue.");
        }
    }

    private static void EnsureDistinct(List<QueueItem> itemList)
    {
        if (itemList.Any(x => x is null)) throw new InvalidQueueEditException("A list holds an item that is not there.");
        if (itemList.Select(x => x.ItemID).Distinct().Count() != itemList.Count) throw new InvalidQueueEditException("An item ID may appear only once in the queue.");
    }

    private static Guid Require(Guid? value, string member)
    {
        return value is { } id && id != Guid.Empty ? id : throw new InvalidQueueEditException($"The edit needs {member}.");
    }

    #endregion -- Checks --
}
