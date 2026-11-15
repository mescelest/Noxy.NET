namespace Noxy.NET.MediaPlayer.Application.Models.Metadata;

public record MetadataLibrary<TResponse>(string ETag, List<TResponse>? ItemList);
