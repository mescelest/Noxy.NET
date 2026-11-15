using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;

namespace Noxy.NET.MediaPlayer.Application.Commands.Metadata;

public record CommandArtistMediaFileListAdd(Guid UserID, Guid ID, List<Guid> MediaFileIDList) : CommandNamedItemMediaFileListAdd<ResponseArtist>(UserID, ID, MediaFileIDList);
