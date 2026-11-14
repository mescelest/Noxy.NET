using Noxy.NET.MediaPlayer.Domain.Responses.Media;

namespace Noxy.NET.MediaPlayer.Application.Interfaces.Services;

public interface IMediaService
{
    // The file name is only used to pick the extension and the default title: it is not stored.
    Task<ResponseMediaFile> Upload(Guid userID, string fileName, string? title, Stream content, CancellationToken cancellationToken = default);
}
