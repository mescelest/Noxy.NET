using Mediator;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Application.Models;
using Noxy.NET.MediaPlayer.Application.Queries.Media;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Extensions;

namespace Noxy.NET.MediaPlayer.Application.Handlers.Media;

public class HandlerMediaFile(IUnitOfWorkFactory factoryUnitOfWork, IFileStorageService serviceFileStorage) : IQueryHandler<QueryMediaFile, MediaDownload>
{
    // Throws MediaNotFoundException for an unknown file, another user's file, or a file that is missing from storage. The caller owns the returned stream and must dispose it.
    public async ValueTask<MediaDownload> Handle(QueryMediaFile query, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = factoryUnitOfWork.Create();

        MediaFile mediaFile = await uow.Media.FindMediaFile(query.ID, query.UserID, cancellationToken) ?? throw new MediaNotFoundException();

        try
        {
            return new(serviceFileStorage.OpenRead(mediaFile.FilePath), mediaFile.ContentType, mediaFile.GetDownloadFileName(), mediaFile.ContentHash);
        }
        catch (FileNotFoundException)
        {
            throw new MediaNotFoundException();
        }
    }
}
