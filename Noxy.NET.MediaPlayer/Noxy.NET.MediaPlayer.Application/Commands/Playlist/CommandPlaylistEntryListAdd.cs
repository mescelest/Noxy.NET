using Mediator;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;

namespace Noxy.NET.MediaPlayer.Application.Commands.Playlist;

public record CommandPlaylistEntryListAdd(Guid UserID, Guid ID, List<Guid> MediaFileIDList, int? Position) : ICommand<ResponsePlaylistDetail>;
