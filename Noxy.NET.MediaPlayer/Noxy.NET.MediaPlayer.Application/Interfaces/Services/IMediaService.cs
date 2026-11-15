using Noxy.NET.MediaPlayer.Application.Models;
using Noxy.NET.MediaPlayer.Domain.Requests.Media;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;

namespace Noxy.NET.MediaPlayer.Application.Interfaces.Services;

public interface IMediaService
{
    // Stages the file and queues a job for it. The file name is only used to pick the extension and the default title.
    Task<ResponseUploadAccepted> Enqueue(Guid userID, string fileName, string? title, Stream content, CancellationToken cancellationToken = default);

    // Changes only the fields that were sent. Throws MediaNotFoundException for an unknown media file or another user's media file.
    Task<ResponseMediaFile> Update(Guid userID, Guid id, RequestUpdateMedia request, CancellationToken cancellationToken = default);

    // Removes the media file and its file. Throws MediaNotFoundException for an unknown media file or another user's media file.
    Task Delete(Guid userID, Guid id, CancellationToken cancellationToken = default);

    // The whole library with its ETag. The list is null when ifNoneMatch already names the current library.
    Task<MediaLibrary> GetMediaLibrary(Guid userID, string? ifNoneMatch, CancellationToken cancellationToken = default);

    // Newest first.
    Task<List<ResponseMediaFile>> GetMediaFileList(Guid userID, int skip, int take, CancellationToken cancellationToken = default);

    // Throws MediaNotFoundException for an unknown file, another user's file, or a file that is missing from storage. The caller owns the returned stream and must dispose it.
    Task<MediaDownload> Open(Guid userID, Guid id, CancellationToken cancellationToken = default);

    // As Open, but the stream is a temporary copy with the media file's current metadata written into its tags, and the stored file is left untouched. Disposing the stream deletes the copy. A file whose tags cannot be written is served as it is.
    Task<MediaDownload> OpenWithTags(Guid userID, Guid id, CancellationToken cancellationToken = default);
}
