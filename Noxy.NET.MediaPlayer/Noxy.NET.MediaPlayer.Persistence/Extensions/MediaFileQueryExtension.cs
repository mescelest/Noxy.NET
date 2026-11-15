using Microsoft.EntityFrameworkCore;
using Noxy.NET.MediaPlayer.Persistence.Tables.Media;

namespace Noxy.NET.MediaPlayer.Persistence.Extensions;

public static class MediaFileQueryExtension
{
    extension(IQueryable<TableMediaFile> query)
    {
        // Everything a media file response is built from: the metadata row, the artists and genres in position order, and the album with its artists.
        public IQueryable<TableMediaFile> IncludeLinks()
        {
            return query
                .Include(x => x.Metadata)
                .Include(x => x.ArtistLinkList.OrderBy(l => l.Position)).ThenInclude(l => l.Artist)
                .Include(x => x.GenreLinkList.OrderBy(l => l.Position)).ThenInclude(l => l.Genre)
                .Include(x => x.Album).ThenInclude(a => a!.ArtistLinkList.OrderBy(l => l.Position)).ThenInclude(l => l.Artist)
                .AsSplitQuery();
        }
    }
}
