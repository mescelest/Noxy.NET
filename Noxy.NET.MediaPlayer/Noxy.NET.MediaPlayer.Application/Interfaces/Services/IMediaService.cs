using Noxy.NET.MediaPlayer.Application.Models;
using Noxy.NET.MediaPlayer.Domain.Requests.Media;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;

namespace Noxy.NET.MediaPlayer.Application.Interfaces.Services;

public interface IMediaService
{
    // Stages the file and queues a job for it. The file name is only used to pick the extension and the default title.
    Task<ResponseUploadAccepted> Enqueue(Guid userID, string fileName, string? title, Stream content, CancellationToken cancellationToken = default);

    // Changes only the fields that were sent. Throws MediaNotFoundException for an unknown song or another user's song.
    Task<ResponseMediaFile> Update(Guid userID, Guid id, RequestUpdateMedia request, CancellationToken cancellationToken = default);

    // Removes the song and its file. Throws MediaNotFoundException for an unknown song or another user's song.
    Task Delete(Guid userID, Guid id, CancellationToken cancellationToken = default);

    // The whole library with its ETag. The list is null when ifNoneMatch already names the current library.
    Task<MediaLibrary> GetMediaLibrary(Guid userID, string? ifNoneMatch, CancellationToken cancellationToken = default);

    // Newest first.
    Task<List<ResponseMediaFile>> GetMediaFileList(Guid userID, int skip, int take, CancellationToken cancellationToken = default);

    // Throws MediaNotFoundException for an unknown file, another user's file, or a file that is missing from storage. The caller owns the returned stream and must dispose it.
    Task<MediaDownload> Open(Guid userID, Guid id, CancellationToken cancellationToken = default);
}
