using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Models;
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

        // The tags the media file's metadata stands for, with its artists, album, album artists and genres loaded. Throws when the media file has no metadata.
        public MediaTagValues ToTagValues()
        {
            EntityMediaFileMetadata metadata = mediaFile.Metadata ?? throw new ArgumentException($"The media file {mediaFile.ID} has no metadata.", nameof(mediaFile));

            return new(metadata.Title, mediaFile.GetArtistNameList(), mediaFile.Album?.Title, mediaFile.GetAlbumArtistNameList(), metadata.Subtitle, metadata.Comment, mediaFile.GetGenreNameList(), metadata.TrackNumber, metadata.DiscNumber, metadata.Year);
        }

        // The text comes from the linked items.
        public ResponseMediaFile ToResponse()
        {
            return new()
            {
                ID = mediaFile.ID,
                Title = mediaFile.Metadata?.Title ?? mediaFile.FilePath,
                ContentType = mediaFile.ContentType,
                SizeBytes = mediaFile.SizeBytes,
                // Stored as UTC, but the database hands it back without a kind, which would serialize without a "Z".
                TimeCreated = DateTime.SpecifyKind(mediaFile.TimeCreated, DateTimeKind.Utc),
                TimeModified = DateTime.SpecifyKind(mediaFile.TimeModified, DateTimeKind.Utc),
                Artist = Join(mediaFile.GetArtistNameList()),
                Album = mediaFile.Album?.Title,
                AlbumArtist = Join(mediaFile.GetAlbumArtistNameList()),
                Subtitle = mediaFile.Metadata?.Subtitle,
                Comment = mediaFile.Metadata?.Comment,
                Genre = Join(mediaFile.GetGenreNameList()),
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

    private static string? Join(List<string> nameList)
    {
        return nameList.Count == 0 ? null : string.Join(NameSeparator, nameList);
    }
}
