using Noxy.NET.MediaPlayer.Presentation.Enums;

namespace Noxy.NET.MediaPlayer.Presentation.Models;

public record PlayerQueueEntry(Guid MediaFileID, FileSyncStateEnum State);
