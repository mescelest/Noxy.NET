using Noxy.NET.MediaPlayer.Application.Handlers.Abstractions.Metadata;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Application.Models.Metadata;
using Noxy.NET.MediaPlayer.Application.Queries.Metadata;
using Noxy.NET.MediaPlayer.Domain.Extensions;
using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;

namespace Noxy.NET.MediaPlayer.Application.Handlers.Metadata.Artist;

public class HandlerArtistList(IUnitOfWorkFactory factoryUnitOfWork, IETagService serviceETag) : BaseHandlerMetadataList<QueryArtistList, ResponseArtist>(factoryUnitOfWork, serviceETag)
{
    protected override Task<List<MetadataStamp>> GetStampList(IUnitOfWork uow, Guid userID, CancellationToken cancellationToken)
    {
        return uow.Metadata.GetArtistStampList(userID, cancellationToken);
    }

    protected override async Task<List<(MetadataStamp Stamp, ResponseArtist Response)>> GetItemList(IUnitOfWork uow, Guid userID, CancellationToken cancellationToken)
    {
        List<ArtistSummary> summaryList = await uow.Metadata.GetArtistSummaryList(userID, cancellationToken);

        return [.. summaryList.Select(x => (new MetadataStamp(x.Artist.ID, x.Artist.TimeModified, x.MediaFileCount), x.Artist.ToResponse(x.MediaFileCount)))];
    }
}
