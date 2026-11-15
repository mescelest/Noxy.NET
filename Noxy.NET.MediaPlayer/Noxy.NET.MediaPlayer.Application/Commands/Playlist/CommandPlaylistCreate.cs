using Mediator;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;

namespace Noxy.NET.MediaPlayer.Application.Commands.Playlist;

public record CommandPlaylistCreate(Guid UserID, string Name, bool AllowDuplicates) : ICommand<ResponsePlaylist>;
