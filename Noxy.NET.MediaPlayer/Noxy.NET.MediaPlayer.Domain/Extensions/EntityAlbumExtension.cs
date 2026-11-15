using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;

namespace Noxy.NET.MediaPlayer.Domain.Extensions;

public static class EntityAlbumExtension
{
    extension(EntityAlbum album)
    {
        public ResponseAlbum ToResponse(int mediaFileCount)
        {
            return new()
            {
                ID = album.ID,
                Title = album.Title,
                ArtistIDList = [.. album.ArtistLinkList.OrderBy(x => x.Position).Select(x => x.ArtistID)],
                MediaFileCount = mediaFileCount,
                TimeModified = DateTime.SpecifyKind(album.TimeModified, DateTimeKind.Utc)
            };
        }
    }
}
