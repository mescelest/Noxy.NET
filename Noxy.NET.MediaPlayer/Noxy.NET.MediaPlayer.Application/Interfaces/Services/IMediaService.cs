using Noxy.NET.MediaPlayer.Application.Models;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;

namespace Noxy.NET.MediaPlayer.Application.Interfaces.Services;

public interface IMediaService
{
    // Stages the file and queues a job for it. The file name is only used to pick the extension and the default title.
    Task<ResponseUploadAccepted> Enqueue(Guid userID, string fileName, string? title, Stream content, CancellationToken cancellationToken = default);

    // Newest first.
    Task<List<ResponseMediaFile>> List(Guid userID, int skip, int take, CancellationToken cancellationToken = default);

    // Throws MediaNotFoundException for an unknown file, another user's file, or a file that is missing from storage. The caller owns the returned stream and must dispose it.
    Task<MediaDownload> Open(Guid userID, Guid id, CancellationToken cancellationToken = default);
}
