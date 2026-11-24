namespace Noxy.NET.MediaPlayer.Presentation.Models;

// How an upload of several files ended: how many are new in the library (and in the playlist when it was an upload to one), how many the library already held (DuplicateCount, which includes the ExistingAddedCount of them that were put in the playlist anyway), and which did not make it.
public record UploadBatchResult(Guid? PlaylistID, int SucceededCount, int DuplicateCount, int ExistingAddedCount, List<UploadFailure> FailureList);
