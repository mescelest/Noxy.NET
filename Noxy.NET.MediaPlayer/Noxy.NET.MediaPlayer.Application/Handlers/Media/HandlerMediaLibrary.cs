using Mediator;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Application.Models;
using Noxy.NET.MediaPlayer.Application.Queries.Media;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Extensions;

namespace Noxy.NET.MediaPlayer.Application.Handlers.Media;

public class HandlerMediaLibrary(IUnitOfWorkFactory factoryUnitOfWork, IETagService serviceETag) : IQueryHandler<QueryMediaLibrary, MediaLibrary>
{
    // The whole library with its ETag. The list is null when IfNoneMatch already names the current library.
    public async ValueTask<MediaLibrary> Handle(QueryMediaLibrary query, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = factoryUnitOfWork.Create();

        if (!string.IsNullOrWhiteSpace(query.IfNoneMatch))
        {
            List<MediaFileStamp> stampList = await uow.Media.GetMediaFileStampList(query.UserID, cancellationToken);
            string current = serviceETag.Compute(stampList.Select(x => (x.ID, x.TimeModified)));
            if (serviceETag.Matches(query.IfNoneMatch, current)) return new(current, null);
        }

        // The ETag comes from the list that is returned, so the two can never disagree.
        List<MediaFile> fileList = await uow.Media.GetMediaFileList(query.UserID, cancellationToken);
        return new(serviceETag.Compute(fileList.Select(x => (x.ID, x.TimeModified))), [.. fileList.Select(x => x.ToResponse())]);
    }
}
