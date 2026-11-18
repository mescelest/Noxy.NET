using Noxy.NET.MediaPlayer.Domain.Responses.Media;

namespace Noxy.NET.MediaPlayer.Presentation.Features.Library;

public record ActionLibraryMediaLoaded(List<ResponseMediaFile> MediaList);
