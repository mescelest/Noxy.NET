using Mediator;
using Noxy.NET.MediaPlayer.Application.Extensions;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Application.Models.Playlist;
using Noxy.NET.MediaPlayer.Application.Queries.Playlist;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Exceptions;
using Noxy.NET.MediaPlayer.Domain.Extensions;

namespace Noxy.NET.MediaPlayer.Application.Handlers.Playlist;

public class HandlerPlaylist(IUnitOfWorkFactory factoryUnitOfWork, IETagService serviceETag) : IQueryHandler<QueryPlaylist, PlaylistView>
{
    public async ValueTask<PlaylistView> Handle(QueryPlaylist query, CancellationToken cancellationToken)
    {
        await using IUnitOfWork uow = factoryUnitOfWork.Create();

        if (!string.IsNullOrWhiteSpace(query.IfNoneMatch))
        {
            EntityPlaylist header = await uow.Playlist.FindPlaylist(query.ID, query.UserID, cancellationToken) ?? throw new PlaylistNotFoundException();
            List<PlaylistStamp> mediaFileStampList = await uow.Playlist.GetEntryMediaFileStampList(query.ID, query.UserID, cancellationToken);
            string current = DetailETag(header.ID, header.TimeModified, mediaFileStampList.Select(x => (x.ID, x.TimeModified)));
            if (serviceETag.Matches(query.IfNoneMatch, current)) return new(current, null);
        }

        // The ETag comes from the entries that are returned, so the two can never disagree.
        EntityPlaylist playlist = await uow.LoadPlaylistWithEntries(query.UserID, query.ID, cancellationToken);
        string eTag = DetailETag(playlist.ID, playlist.TimeModified, playlist.EntryList.Select(x => (x.MediaFileID, x.GetLoadedMediaFile().TimeModified)));
        return new(eTag, playlist.ToDetailResponse());
    }

    // The ETag of a playlist with its entries moves with the playlist and with every media file in it, so a tag edit is never answered with a 304.
    private string DetailETag(Guid playlistID, DateTime playlistModified, IEnumerable<(Guid ID, DateTime TimeModified)> mediaFileStampList)
    {
        return serviceETag.Compute([(playlistID, playlistModified), .. mediaFileStampList]);
    }
}
