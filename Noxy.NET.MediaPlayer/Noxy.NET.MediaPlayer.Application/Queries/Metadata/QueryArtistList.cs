using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;

namespace Noxy.NET.MediaPlayer.Application.Queries.Metadata;

public record QueryArtistList(Guid UserID, string? IfNoneMatch) : QueryMetadataList<ResponseArtist>(UserID, IfNoneMatch);
