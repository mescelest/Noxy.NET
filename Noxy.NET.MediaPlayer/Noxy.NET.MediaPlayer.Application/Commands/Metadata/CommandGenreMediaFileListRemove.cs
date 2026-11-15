using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;

namespace Noxy.NET.MediaPlayer.Application.Commands.Metadata;

public record CommandGenreMediaFileListRemove(Guid UserID, Guid ID, List<Guid> MediaFileIDList) : CommandNamedItemMediaFileListRemove<ResponseGenre>(UserID, ID, MediaFileIDList);
