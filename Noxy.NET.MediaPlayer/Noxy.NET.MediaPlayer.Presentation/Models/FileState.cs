using Noxy.NET.MediaPlayer.Presentation.Enums;

namespace Noxy.NET.MediaPlayer.Presentation.Models;

public record FileState(Guid MediaFileID, FileSyncStateEnum State, string? Error, DateTime? TagTimeModified = null);
