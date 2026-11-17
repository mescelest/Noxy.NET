using Noxy.NET.MediaPlayer.Domain.Models;

namespace Noxy.NET.MediaPlayer.Presentation.Models;

// Where a new item goes in the source part: after AfterItemID, or at the front of the source part when that is null.
public record QueuePlacement(QueueItem Item, Guid? AfterItemID);
