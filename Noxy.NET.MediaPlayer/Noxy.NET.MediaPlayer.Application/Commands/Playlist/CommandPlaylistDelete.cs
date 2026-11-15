using Mediator;

namespace Noxy.NET.MediaPlayer.Application.Commands.Playlist;

public record CommandPlaylistDelete(Guid UserID, Guid ID) : ICommand;
