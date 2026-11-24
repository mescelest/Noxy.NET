using Noxy.NET.MediaPlayer.Application.Handlers.Abstractions.Metadata;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Application.Models.Metadata;
using Noxy.NET.MediaPlayer.Application.Queries.Metadata;
using Noxy.NET.MediaPlayer.Domain.Extensions;
using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;

namespace Noxy.NET.MediaPlayer.Application.Handlers.Metadata.Album;

public class HandlerAlbumList(IUnitOfWorkFactory factoryUnitOfWork, IETagService serviceETag) : BaseHandlerMetadataList<QueryAlbumList, ResponseAlbum>(factoryUnitOfWork, serviceETag)
{
    protected override Task<List<MetadataStamp>> GetStampList(IUnitOfWork uow, Guid userID, CancellationToken cancellationToken)
    {
        return uow.Metadata.GetAlbumStampList(userID, cancellationToken);
    }

    protected override async Task<List<(MetadataStamp Stamp, ResponseAlbum Response)>> GetItemList(IUnitOfWork uow, Guid userID, CancellationToken cancellationToken)
    {
        List<AlbumSummary> summaryList = await uow.Metadata.GetAlbumSummaryList(userID, cancellationToken);

        return [.. summaryList.Select(x => (new MetadataStamp(x.Album.ID, x.Album.TimeModified, x.MediaFileCount, x.Album.TimeArtModified), x.Album.ToResponse(x.MediaFileCount)))];
    }
}
