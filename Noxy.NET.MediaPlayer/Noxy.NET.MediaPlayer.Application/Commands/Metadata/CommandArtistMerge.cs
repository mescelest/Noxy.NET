using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;

namespace Noxy.NET.MediaPlayer.Application.Commands.Metadata;

public record CommandArtistMerge(Guid UserID, Guid ID, Guid TargetID) : CommandNamedItemMerge<ResponseArtist>(UserID, ID, TargetID);
