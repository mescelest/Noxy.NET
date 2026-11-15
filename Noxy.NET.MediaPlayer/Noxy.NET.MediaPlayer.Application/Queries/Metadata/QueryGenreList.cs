using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;

namespace Noxy.NET.MediaPlayer.Application.Queries.Metadata;

public record QueryGenreList(Guid UserID, string? IfNoneMatch) : QueryMetadataList<ResponseGenre>(UserID, IfNoneMatch);
