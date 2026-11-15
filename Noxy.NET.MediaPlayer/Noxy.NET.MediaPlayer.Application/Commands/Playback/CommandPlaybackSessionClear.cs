using Mediator;

namespace Noxy.NET.MediaPlayer.Application.Commands.Playback;

public record CommandPlaybackSessionClear(Guid UserID) : ICommand;
