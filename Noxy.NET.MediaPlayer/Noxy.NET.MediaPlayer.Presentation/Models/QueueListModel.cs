namespace Noxy.NET.MediaPlayer.Presentation.Models;

// The rows of the queue page in order, and where the song that is playing is (-1 when nothing is).
public sealed record QueueListModel(List<QueueListRow> RowList, int CurrentIndex);
