using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Responses.Metadata;

namespace Noxy.NET.MediaPlayer.Domain.Extensions;

public static class EntityArtistExtension
{
    extension(EntityArtist artist)
    {
        public ResponseArtist ToResponse(int mediaFileCount)
        {
            return new()
            {
                ID = artist.ID,
                Name = artist.Name,
                MediaFileCount = mediaFileCount,
                Weight = artist.Weight,
                // Stored as UTC, but the database hands it back without a kind, which would serialize without a "Z".
                TimeModified = DateTime.SpecifyKind(artist.TimeModified, DateTimeKind.Utc)
            };
        }
    }
}
