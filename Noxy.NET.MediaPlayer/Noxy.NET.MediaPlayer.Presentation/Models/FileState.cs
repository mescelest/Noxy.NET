namespace Noxy.NET.MediaPlayer.Presentation.Models;

// VerifiedHash is the contentHash the stored bytes were checked against: set only when the state is OnDevice. Error is the reason a download Failed.
public record FileState(Guid MediaFileID, FileSyncState State, string? VerifiedHash, string? Error);
