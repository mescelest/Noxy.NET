using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;

namespace Noxy.NET.MediaPlayer.Application.Queries.Metadata;

public record QueryAlbumList(Guid UserID, string? IfNoneMatch) : QueryMetadataList<ResponseAlbum>(UserID, IfNoneMatch);
