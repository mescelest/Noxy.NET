using Noxy.NET.MediaPlayer.Domain.Responses.Media;

namespace Noxy.NET.MediaPlayer.Application.Models.Metadata;

public record MetadataChangeResult<TResponse>(TResponse Response, bool Changed, MetadataChangeSet ChangeSet, List<ResponseMediaFile> MediaFileList);
