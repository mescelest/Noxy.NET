using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;

namespace Noxy.NET.MediaPlayer.Application.Commands.Metadata;

public record CommandArtistMediaFileListRemove(Guid UserID, Guid ID, List<Guid> MediaFileIDList) : CommandNamedItemMediaFileListRemove<ResponseArtist>(UserID, ID, MediaFileIDList);
