using Mediator;
using Noxy.NET.MediaPlayer.Application.Interfaces;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Application.Models.Metadata;
using Noxy.NET.MediaPlayer.Application.Queries.Metadata;
using Noxy.NET.MediaPlayer.Domain.Entities;

namespace Noxy.NET.MediaPlayer.Application.Handlers.Metadata.Album;

public class HandlerAlbumArt(IUnitOfWorkFactory factoryUnitOfWork, IETagService serviceETag, IArtStorageService serviceArtStorage) : IQueryHandler<QueryAlbumArt, AlbumArtPicture?>
{
    public async ValueTask<AlbumArtPicture?> Handle(QueryAlbumArt query, CancellationToken cancellationToken)
    {
        EntityAlbum? album;
        await using (IUnitOfWork uow = factoryUnitOfWork.Create())
        {
            album = await uow.Metadata.FindAlbum(query.AlbumID, query.UserID, cancellationToken);
        }

        if (album?.TimeArtModified is not { } timeArtModified) return null;

        // The picture only ever changes with its time, so the time is the whole stamp.
        string eTag = serviceETag.Compute([(album.ID, timeArtModified)]);
        if (!string.IsNullOrWhiteSpace(query.IfNoneMatch) && serviceETag.Matches(query.IfNoneMatch, eTag)) return new(eTag, null);

        await using Stream? stream = serviceArtStorage.OpenAlbumArt(album.ID);
        if (stream is null) return null;

        using MemoryStream copy = new();
        await stream.CopyToAsync(copy, cancellationToken);
        return new(eTag, copy.ToArray());
    }
}
