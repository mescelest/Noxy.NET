using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;

namespace Noxy.NET.MediaPlayer.Application.Commands.Metadata;

public record CommandArtistSplit(Guid UserID, Guid ID, List<string> NameList) : CommandNamedItemSplit<ResponseArtist>(UserID, ID, NameList);
