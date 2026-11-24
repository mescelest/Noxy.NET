using Mediator;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;

namespace Noxy.NET.MediaPlayer.Application.Commands.Playlist;

public record CommandPlaylistMove(Guid UserID, Guid ID, int Position) : ICommand<List<ResponsePlaylist>>;
