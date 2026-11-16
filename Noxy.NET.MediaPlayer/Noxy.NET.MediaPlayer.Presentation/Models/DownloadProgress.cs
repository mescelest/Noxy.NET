namespace Noxy.NET.MediaPlayer.Presentation.Models;

// The running or last bulk download. Total counts the media files queued; Finished and Failed count the ones that ended; BytesDone is the size of the finished ones. StoppedReason is set when the bulk gave up on the rest, which then stay NotDownloaded.
public record DownloadProgress(int Total, int Finished, int Failed, long BytesDone, DownloadStopReason? StoppedReason);
