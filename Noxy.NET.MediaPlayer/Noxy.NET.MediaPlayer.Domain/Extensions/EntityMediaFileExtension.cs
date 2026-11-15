using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;

namespace Noxy.NET.MediaPlayer.Domain.Extensions;

public static class EntityMediaFileExtension
{
    extension(EntityMediaFile mediaFile)
    {
        // The title with the extension of the stored file, or the stored file name when there is no metadata.
        public string GetDownloadFileName()
        {
            return mediaFile.Metadata is null ? mediaFile.FilePath : $"{mediaFile.Metadata.Title}{Path.GetExtension(mediaFile.FilePath)}";
        }

        public ResponseMediaFile ToResponse()
        {
            return new()
            {
                ID = mediaFile.ID,
                Title = mediaFile.Metadata?.Title ?? mediaFile.FilePath,
                ContentType = mediaFile.ContentType,
                SizeBytes = mediaFile.SizeBytes,
                ContentHash = mediaFile.ContentHash,
                // Stored as UTC, but the database hands it back without a kind, which would serialize without a "Z".
                TimeCreated = DateTime.SpecifyKind(mediaFile.TimeCreated, DateTimeKind.Utc),
                TimeModified = DateTime.SpecifyKind(mediaFile.TimeModified, DateTimeKind.Utc),
                Artist = mediaFile.Metadata?.Artist,
                Album = mediaFile.Metadata?.Album,
                AlbumArtist = mediaFile.Metadata?.AlbumArtist,
                Subtitle = mediaFile.Metadata?.Subtitle,
                Comment = mediaFile.Metadata?.Comment,
                Genre = mediaFile.Metadata?.Genre,
                TrackNumber = mediaFile.Metadata?.TrackNumber,
                DiscNumber = mediaFile.Metadata?.DiscNumber,
                Year = mediaFile.Metadata?.Year,
                Duration = mediaFile.Metadata?.Duration
            };
        }
    }
}
