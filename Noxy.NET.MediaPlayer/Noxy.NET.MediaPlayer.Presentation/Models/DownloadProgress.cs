using Noxy.NET.MediaPlayer.Presentation.Enums;

namespace Noxy.NET.MediaPlayer.Presentation.Models;

public record DownloadProgress(int Total, int Finished, int Failed, long BytesDone, DownloadStopReasonEnum? StoppedReason);
