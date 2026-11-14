using Noxy.NET.MediaPlayer.Domain.Responses.Media;

namespace Noxy.NET.MediaPlayer.Application.Models;

// The list is null when the caller already has this exact library, so there is nothing to send.
public record MediaLibrary(string ETag, List<ResponseMediaFile>? MediaFileList);
