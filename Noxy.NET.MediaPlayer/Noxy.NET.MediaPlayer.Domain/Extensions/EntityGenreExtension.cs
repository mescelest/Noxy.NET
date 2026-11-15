using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;

namespace Noxy.NET.MediaPlayer.Domain.Extensions;

public static class EntityGenreExtension
{
    extension(EntityGenre genre)
    {
        public ResponseGenre ToResponse(int mediaFileCount)
        {
            return new()
            {
                ID = genre.ID,
                Name = genre.Name,
                MediaFileCount = mediaFileCount,
                TimeModified = DateTime.SpecifyKind(genre.TimeModified, DateTimeKind.Utc)
            };
        }
    }
}
