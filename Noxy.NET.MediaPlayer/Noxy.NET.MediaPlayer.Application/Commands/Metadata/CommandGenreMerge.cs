using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;

namespace Noxy.NET.MediaPlayer.Application.Commands.Metadata;

public record CommandGenreMerge(Guid UserID, Guid ID, Guid TargetID) : CommandNamedItemMerge<ResponseGenre>(UserID, ID, TargetID);
