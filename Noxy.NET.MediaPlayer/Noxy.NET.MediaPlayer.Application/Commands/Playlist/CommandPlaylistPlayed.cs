using Mediator;

namespace Noxy.NET.MediaPlayer.Application.Commands.Playlist;

public record CommandPlaylistPlayed(Guid UserID, Guid ID) : ICommand;
