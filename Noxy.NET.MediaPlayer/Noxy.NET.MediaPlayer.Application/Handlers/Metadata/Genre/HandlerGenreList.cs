using Noxy.NET.MediaPlayer.Application.Handlers.Abstractions.Metadata;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Application.Models.Metadata;
using Noxy.NET.MediaPlayer.Application.Queries.Metadata;
using Noxy.NET.MediaPlayer.Domain.Extensions;
using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;

namespace Noxy.NET.MediaPlayer.Application.Handlers.Metadata.Genre;

public class HandlerGenreList(IUnitOfWorkFactory factoryUnitOfWork, IETagService serviceETag) : BaseHandlerMetadataList<QueryGenreList, ResponseGenre>(factoryUnitOfWork, serviceETag)
{
    protected override Task<List<MetadataStamp>> GetStampList(IUnitOfWork uow, Guid userID, CancellationToken cancellationToken)
    {
        return uow.Metadata.GetGenreStampList(userID, cancellationToken);
    }

    protected override async Task<List<(MetadataStamp Stamp, ResponseGenre Response)>> GetItemList(IUnitOfWork uow, Guid userID, CancellationToken cancellationToken)
    {
        List<GenreSummary> summaryList = await uow.Metadata.GetGenreSummaryList(userID, cancellationToken);

        return [.. summaryList.Select(x => (new MetadataStamp(x.Genre.ID, x.Genre.TimeModified, x.MediaFileCount), x.Genre.ToResponse(x.MediaFileCount)))];
    }
}
