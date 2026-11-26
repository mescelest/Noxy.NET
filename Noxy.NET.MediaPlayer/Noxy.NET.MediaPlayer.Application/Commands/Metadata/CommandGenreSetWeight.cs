using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;

namespace Noxy.NET.MediaPlayer.Application.Commands.Metadata;

public record CommandGenreSetWeight(Guid UserID, Guid ID, int Weight) : CommandNamedItemSetWeight<ResponseGenre>(UserID, ID, Weight);
