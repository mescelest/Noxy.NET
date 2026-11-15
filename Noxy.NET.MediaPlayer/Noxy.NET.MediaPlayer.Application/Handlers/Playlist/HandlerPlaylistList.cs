using Mediator;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Application.Models.Playlist;
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
            string current = serviceETag.Compute(stampList.SelectMany(x => ETagStamps(x.ID, x.TimeModified, x.TimeLastPlayed)));
            if (serviceETag.Matches(query.IfNoneMatch, current)) return new(current, null);
        }

        // The ETag comes from the list that is returned, so the two can never disagree.
        List<PlaylistSummary> summaryList = await uow.Playlist.GetPlaylistSummaryList(query.UserID, cancellationToken);
        return new(serviceETag.Compute(summaryList.SelectMany(x => ETagStamps(x.Playlist.ID, x.Playlist.TimeModified, x.Playlist.TimeLastPlayed))), [.. summaryList.Select(x => x.Playlist.ToResponse(x.EntryCount))]);
    }

    // A play moves the list ETag, so a device refetches the list for the new stamp, but it is not part of the detail ETag.
    private static List<(Guid ID, DateTime TimeModified)> ETagStamps(Guid id, DateTime timeModified, DateTime? timeLastPlayed)
    {
        return [(id, timeModified), (id, timeLastPlayed ?? DateTime.MinValue)];
    }
}
