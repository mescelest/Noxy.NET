using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;

namespace Noxy.NET.MediaPlayer.Application.Commands.Metadata;

public record CommandGenreSplit(Guid UserID, Guid ID, List<string> NameList) : CommandNamedItemSplit<ResponseGenre>(UserID, ID, NameList);
