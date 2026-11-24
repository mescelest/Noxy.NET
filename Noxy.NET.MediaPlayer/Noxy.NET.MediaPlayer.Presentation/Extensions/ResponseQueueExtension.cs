using Noxy.NET.MediaPlayer.Domain.Models;
using Noxy.NET.MediaPlayer.Domain.Responses.Remote;
using Noxy.NET.MediaPlayer.Presentation.Enums;
using Noxy.NET.MediaPlayer.Presentation.Models;

namespace Noxy.NET.MediaPlayer.Presentation.Extensions;

public static class ResponseQueueExtension
{
    extension(ResponseQueue queue)
    {
        // The queue as one list in play order: what was played (no heading), the song that is playing, the songs the user queued, then the songs that come from the source. A part with no songs has no heading.
        public QueueListModel ToListModel(string? sourceName)
        {
            List<QueueListRow> rowList = new(queue.HistoryList.Count + queue.UserQueueList.Count + queue.SourceList.Count + 3);
            rowList.AddRange(queue.HistoryList.Select(x => new QueueListRow(QueueRowKindEnum.History, x, null)));

            int currentIndex = -1;
            if (queue.Current is { } current)
            {
                rowList.Add(new(QueueRowKindEnum.DividerCurrent, null, null));
                currentIndex = rowList.Count;
                rowList.Add(new(QueueRowKindEnum.Current, current, null));
            }

            AddPart(rowList, queue.UserQueueList, QueueRowKindEnum.DividerUserQueue, QueueRowKindEnum.UserQueue, null);
            AddPart(rowList, queue.SourceList, QueueRowKindEnum.DividerSource, QueueRowKindEnum.Source, sourceName);

            return new(rowList, currentIndex);
        }
    }

    private static void AddPart(List<QueueListRow> rowList, List<QueueItem> itemList, QueueRowKindEnum dividerKind, QueueRowKindEnum itemKind, string? sourceName)
    {
        if (itemList.Count == 0) return;

        rowList.Add(new(dividerKind, null, sourceName));
        rowList.AddRange(itemList.Select(x => new QueueListRow(itemKind, x, null)));
    }
}
