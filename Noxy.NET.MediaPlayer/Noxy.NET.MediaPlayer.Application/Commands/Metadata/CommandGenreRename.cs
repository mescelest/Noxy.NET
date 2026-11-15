using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;

namespace Noxy.NET.MediaPlayer.Application.Commands.Metadata;

public record CommandGenreRename(Guid UserID, Guid ID, string Name) : CommandNamedItemRename<ResponseGenre>(UserID, ID, Name);
