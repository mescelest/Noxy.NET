using Mediator;
using Noxy.NET.MediaPlayer.Domain.Responses.Playback;

namespace Noxy.NET.MediaPlayer.Application.Commands.Playback;

public record CommandPlaybackSessionSave(Guid UserID, List<Guid> QueueMediaFileIDList, int? CurrentIndex, double PositionSeconds, Guid? PlaylistID) : ICommand<ResponsePlaybackSession>;
