using Mediator;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;

namespace Noxy.NET.MediaPlayer.Application.Commands.Playlist;

public record CommandPlaylistEntryMove(Guid UserID, Guid ID, Guid EntryID, int Position) : ICommand<ResponsePlaylistDetail>;
