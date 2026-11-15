using Mediator;
using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;

namespace Noxy.NET.MediaPlayer.Application.Commands.Metadata;

public record CommandAlbumMediaFileListRemove(Guid UserID, Guid ID, List<Guid> MediaFileIDList) : ICommand<ResponseAlbum>;
