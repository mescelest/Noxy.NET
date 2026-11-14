using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;

namespace Noxy.NET.MediaPlayer.Application.Services;

public static class MediaFileMapper
{
    public static ResponseMediaFile ToResponse(MediaFile mediaFile)
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
            TimeModified = DateTime.SpecifyKind(mediaFile.TimeModified, DateTimeKind.Utc)
        };
    }
}
