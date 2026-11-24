using Noxy.NET.MediaPlayer.Domain.Models;
using Noxy.NET.MediaPlayer.Presentation.Enums;

namespace Noxy.NET.MediaPlayer.Presentation.Models;

// A row of the queue page: a heading (no item) or a song. The source name is only on the heading of the source part.
public sealed record QueueListRow(QueueRowKindEnum Kind, QueueItem? Item, string? SourceName);
