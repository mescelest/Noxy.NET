using Noxy.NET.MediaPlayer.Domain.Responses.Media;

namespace Noxy.NET.Test.MediaPlayer.Fixtures;

public static class MediaFileFactory
{
    public static ResponseMediaFile Create(string title = "Media", string? artist = null, string? album = null, int? year = null, DateTime? timeCreated = null, Guid? id = null)
    {
        return new()
        {
            ID = id ?? Guid.CreateVersion7(),
            Title = title,
            Artist = artist,
            Album = album,
            Year = year,
            ContentType = "audio/mpeg",
            SizeBytes = 3,
            ContentHash = new('a', 64),
            TimeCreated = timeCreated ?? DateTime.UtcNow,
            TimeModified = DateTime.UtcNow
        };
    }
}
