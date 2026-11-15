using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;

namespace Noxy.NET.MediaPlayer.Application.Commands.Metadata;

public record CommandGenreMediaFileListAdd(Guid UserID, Guid ID, List<Guid> MediaFileIDList) : CommandNamedItemMediaFileListAdd<ResponseGenre>(UserID, ID, MediaFileIDList);
