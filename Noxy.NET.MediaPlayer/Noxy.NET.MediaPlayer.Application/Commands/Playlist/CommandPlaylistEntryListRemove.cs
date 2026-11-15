using Mediator;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;

namespace Noxy.NET.MediaPlayer.Application.Commands.Playlist;

public record CommandPlaylistEntryListRemove(Guid UserID, Guid ID, List<Guid> EntryIDList) : ICommand<ResponsePlaylistDetail>;
