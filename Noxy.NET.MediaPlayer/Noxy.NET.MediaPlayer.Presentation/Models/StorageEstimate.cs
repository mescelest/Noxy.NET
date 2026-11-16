namespace Noxy.NET.MediaPlayer.Presentation.Models;

// UsedBytes is what the file store holds; FreeBytes is the free space on the volume it is on.
public record StorageEstimate(long UsedBytes, long FreeBytes);
