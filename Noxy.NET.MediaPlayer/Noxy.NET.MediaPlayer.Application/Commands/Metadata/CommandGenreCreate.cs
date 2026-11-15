using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;

namespace Noxy.NET.MediaPlayer.Application.Commands.Metadata;

public record CommandGenreCreate(Guid UserID, string Name) : CommandNamedItemCreate<ResponseGenre>(UserID, Name);
