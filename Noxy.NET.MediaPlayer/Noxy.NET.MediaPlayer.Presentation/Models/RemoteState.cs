using Noxy.NET.MediaPlayer.Domain.Responses.Remote;

namespace Noxy.NET.MediaPlayer.Presentation.Models;

public record RemoteState(ResponsePlayerState State, DateTime ReceivedAt);
