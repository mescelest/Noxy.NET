using Mediator;
using Microsoft.Extensions.Logging;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Application.Models.Media;
using Noxy.NET.MediaPlayer.Application.Queries.Media;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Extensions;

namespace Noxy.NET.MediaPlayer.Application.Handlers.Media;

public class HandlerMediaFileWithTags(IUnitOfWorkFactory factoryUnitOfWork, IFileStorageService serviceFileStorage, IMediaTagWritingService serviceMediaTagWriting, ILogger<HandlerMediaFileWithTags> logger) : IQueryHandler<QueryMediaFileWithTags, MediaDownload>
{
    // A temporary copy with the media file's current metadata written into its tags, and the stored file is left untouched. Disposing the stream deletes the copy. A file whose tags cannot be written is served as it is.
    public async ValueTask<MediaDownload> Handle(QueryMediaFileWithTags query, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = factoryUnitOfWork.Create();

        EntityMediaFile mediaFile = await uow.Media.FindMediaFile(query.ID, query.UserID, cancellationToken) ?? throw new MediaNotFoundException();

        string path = CopyToTemporary(mediaFile);
        try
        {
            if (mediaFile.Metadata is null || TryWriteTags(path, mediaFile.Metadata))
            {
                return new(serviceFileStorage.OpenTemporary(path), mediaFile.ContentType, mediaFile.GetDownloadFileName(), mediaFile.ContentHash);
            }

            // A failed write can leave the copy half written, so serve a fresh one.
            serviceFileStorage.DeleteTemporary(path);
            path = CopyToTemporary(mediaFile);

            return new(serviceFileStorage.OpenTemporary(path), mediaFile.ContentType, mediaFile.GetDownloadFileName(), mediaFile.ContentHash);
        }
        catch
        {
            serviceFileStorage.DeleteTemporary(path);
            throw;
        }
    }

    private string CopyToTemporary(EntityMediaFile mediaFile)
    {
        try
        {
            return serviceFileStorage.CopyToTemporary(mediaFile.FilePath);
        }
        catch (FileNotFoundException)
        {
            throw new MediaNotFoundException();
        }
    }

    // A file the tagger cannot read is still a media file the user is entitled to, so the failure is logged and the untagged copy is served.
    private bool TryWriteTags(string path, EntityMediaFileMetadata metadata)
    {
        try
        {
            serviceMediaTagWriting.Write(path, metadata);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not write the tags of media file {MediaFileID}; serving it untagged", metadata.MediaFileID);
            return false;
        }
    }
}
