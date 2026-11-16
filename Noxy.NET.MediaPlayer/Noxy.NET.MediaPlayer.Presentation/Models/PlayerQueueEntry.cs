using Noxy.NET.MediaPlayer.Presentation.Enums;

namespace Noxy.NET.MediaPlayer.Presentation.Models;

// State is what the media file's file is like on the device, or Failed when the entry could not be played (its download failed or the player could not decode it).
public record PlayerQueueEntry(Guid MediaFileID, FileSyncStateEnum State);
