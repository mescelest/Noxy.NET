using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;

namespace Noxy.NET.MediaPlayer.Domain.Extensions;

public static class EntityMediaFileExtension
{
    private const string NameSeparator = "; ";

    extension(EntityMediaFile mediaFile)
    {
        // The title with the extension of the stored file, or the stored file name when there is no metadata.
        public string GetDownloadFileName()
        {
            return mediaFile.Metadata is null ? mediaFile.FilePath : $"{mediaFile.Metadata.Title}{Path.GetExtension(mediaFile.FilePath)}";
        }

        // The artist names in position order. A link whose artist was not loaded is a bug in the caller.
        public List<string> GetArtistNameList()
        {
            return [.. mediaFile.ArtistLinkList.OrderBy(x => x.Position).Select(x => x.Artist?.Name ?? throw new InvalidOperationException($"The media file {mediaFile.ID} was loaded without its artists."))];
        }

        public List<string> GetGenreNameList()
        {
            return [.. mediaFile.GenreLinkList.OrderBy(x => x.Position).Select(x => x.Genre?.Name ?? throw new InvalidOperationException($"The media file {mediaFile.ID} was loaded without its genres."))];
        }

        // The album's artists in position order.
        public List<string> GetAlbumArtistNameList()
        {
            if (mediaFile.Album is null) return [];

            return [.. mediaFile.Album.ArtistLinkList.OrderBy(x => x.Position).Select(x => x.Artist?.Name ?? throw new InvalidOperationException($"The media file {mediaFile.ID} was loaded without the artists of its album."))];
        }

        // What the file shows for each kind: the linked names, or for a file that has none of that kind yet (the library is filled in at start-up) the single text it had before items existed.
        public List<string> GetEffectiveArtistNameList()
        {
            return WithLegacy(mediaFile.GetArtistNameList(), mediaFile.Metadata?.Artist);
        }

        public List<string> GetEffectiveAlbumArtistNameList()
        {
            return WithLegacy(mediaFile.GetAlbumArtistNameList(), mediaFile.Metadata?.AlbumArtist);
        }

        public List<string> GetEffectiveGenreNameList()
        {
            return WithLegacy(mediaFile.GetGenreNameList(), mediaFile.Metadata?.Genre);
        }

        public string? GetEffectiveAlbumTitle()
        {
            return mediaFile.Album?.Title ?? mediaFile.Metadata?.Album;
        }

        // The text comes from the linked items. A file that has none of a kind yet (the library is filled in at start-up) still shows the text it had before items existed.
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
                Artist = Join(mediaFile.GetEffectiveArtistNameList()),
                Album = mediaFile.GetEffectiveAlbumTitle(),
                AlbumArtist = Join(mediaFile.GetEffectiveAlbumArtistNameList()),
                Subtitle = mediaFile.Metadata?.Subtitle,
                Comment = mediaFile.Metadata?.Comment,
                Genre = Join(mediaFile.GetEffectiveGenreNameList()),
                TrackNumber = mediaFile.Metadata?.TrackNumber,
                DiscNumber = mediaFile.Metadata?.DiscNumber,
                Year = mediaFile.Metadata?.Year,
                Duration = mediaFile.Metadata?.Duration,
                ArtistIDList = [.. mediaFile.ArtistLinkList.OrderBy(x => x.Position).Select(x => x.ArtistID)],
                AlbumID = mediaFile.AlbumID,
                AlbumArtistIDList = mediaFile.Album is null ? [] : [.. mediaFile.Album.ArtistLinkList.OrderBy(x => x.Position).Select(x => x.ArtistID)],
                GenreIDList = [.. mediaFile.GenreLinkList.OrderBy(x => x.Position).Select(x => x.GenreID)]
            };
        }
    }

    private static List<string> WithLegacy(List<string> nameList, string? legacy)
    {
        return nameList.Count > 0 || legacy is null ? nameList : [legacy];
    }

    private static string? Join(List<string> nameList)
    {
        return nameList.Count == 0 ? null : string.Join(NameSeparator, nameList);
    }
}
