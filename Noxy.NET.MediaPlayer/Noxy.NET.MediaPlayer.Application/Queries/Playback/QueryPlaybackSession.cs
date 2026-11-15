using Mediator;
using Noxy.NET.MediaPlayer.Application.Models.Playback;

namespace Noxy.NET.MediaPlayer.Application.Queries.Playback;

public record QueryPlaybackSession(Guid UserID, string? IfNoneMatch) : IQuery<PlaybackSessionView>;
