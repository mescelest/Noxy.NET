using Mediator;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Application.Models;
using Noxy.NET.MediaPlayer.Application.Queries.Playlist;
using Noxy.NET.MediaPlayer.Domain.Extensions;

namespace Noxy.NET.MediaPlayer.Application.Handlers.Playlist;

public class HandlerPlaylistList(IUnitOfWorkFactory factoryUnitOfWork, IETagService serviceETag) : IQueryHandler<QueryPlaylistList, PlaylistLibrary>
{
    public async ValueTask<PlaylistLibrary> Handle(QueryPlaylistList query, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = factoryUnitOfWork.Create();

        if (!string.IsNullOrWhiteSpace(query.IfNoneMatch))
        {
            List<PlaylistStamp> stampList = await uow.Playlist.GetPlaylistStampList(query.UserID, cancellationToken);
            string current = serviceETag.Compute(stampList.Select(x => (x.ID, x.TimeModified)));
            if (serviceETag.Matches(query.IfNoneMatch, current)) return new(current, null);
        }

        // The ETag comes from the list that is returned, so the two can never disagree.
        List<PlaylistSummary> summaryList = await uow.Playlist.GetPlaylistSummaryList(query.UserID, cancellationToken);
        return new(serviceETag.Compute(summaryList.Select(x => (x.Playlist.ID, x.Playlist.TimeModified))), [.. summaryList.Select(x => x.Playlist.ToResponse(x.EntryCount))]);
    }
}
