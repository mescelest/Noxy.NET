using Noxy.NET.MediaPlayer.Domain.Responses.Remote;

namespace Noxy.NET.MediaPlayer.Presentation.Models;

public record LocalQueueSnapshot(ResponseQueue Queue, double PositionSeconds, DateTime SavedAt);
