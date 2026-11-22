namespace Noxy.NET.MediaPlayer.Presentation.Models;

// How an upload of several files ended: how many are in the library (and in the playlist when it was an upload to one), and which did not make it.
public record UploadBatchResult(Guid? PlaylistID, int SucceededCount, List<UploadFailure> FailureList);
