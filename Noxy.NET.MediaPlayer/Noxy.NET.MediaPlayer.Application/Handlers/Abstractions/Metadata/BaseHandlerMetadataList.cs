using Mediator;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Application.Models.Metadata;
using Noxy.NET.MediaPlayer.Application.Queries.Metadata;

namespace Noxy.NET.MediaPlayer.Application.Handlers.Abstractions.Metadata;

public abstract class BaseHandlerMetadataList<TQuery, TResponse>(IUnitOfWorkFactory factoryUnitOfWork, IETagService serviceETag) : IQueryHandler<TQuery, MetadataLibrary<TResponse>> where TQuery : QueryMetadataList<TResponse>
{
    public async ValueTask<MetadataLibrary<TResponse>> Handle(TQuery query, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = factoryUnitOfWork.Create();

        if (!string.IsNullOrWhiteSpace(query.IfNoneMatch))
        {
            List<MetadataStamp> stampList = await GetStampList(uow, query.UserID, cancellationToken);
            string current = Compute(stampList);
            if (serviceETag.Matches(query.IfNoneMatch, current)) return new(current, null);
        }

        // The ETag comes from the list that is returned, so the two can never disagree.
        List<(MetadataStamp Stamp, TResponse Response)> itemList = await GetItemList(uow, query.UserID, cancellationToken);
        return new(Compute(itemList.Select(x => x.Stamp)), [.. itemList.Select(x => x.Response)]);
    }

    protected abstract Task<List<MetadataStamp>> GetStampList(IUnitOfWork uow, Guid userID, CancellationToken cancellationToken);

    // Newest first.
    protected abstract Task<List<(MetadataStamp Stamp, TResponse Response)>> GetItemList(IUnitOfWork uow, Guid userID, CancellationToken cancellationToken);

    private string Compute(IEnumerable<MetadataStamp> stampList)
    {
        // The art time rides along as a second stamp of the item, so a new picture moves the ETag.
        return serviceETag.ComputeCounted(stampList.SelectMany(x => new[] { (x.ID, x.TimeModified, x.MediaFileCount), (x.ID, x.TimeArtModified ?? DateTime.MinValue, x.MediaFileCount) }));
    }
}
