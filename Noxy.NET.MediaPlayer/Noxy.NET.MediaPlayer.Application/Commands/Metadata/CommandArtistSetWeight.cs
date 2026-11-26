using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;

namespace Noxy.NET.MediaPlayer.Application.Commands.Metadata;

public record CommandArtistSetWeight(Guid UserID, Guid ID, int Weight) : CommandNamedItemSetWeight<ResponseArtist>(UserID, ID, Weight);
