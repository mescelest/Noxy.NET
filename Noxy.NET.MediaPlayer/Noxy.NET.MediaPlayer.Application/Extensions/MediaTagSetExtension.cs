using Noxy.NET.MediaPlayer.Application.Models.Metadata;
using Noxy.NET.MediaPlayer.Domain.Models;

namespace Noxy.NET.MediaPlayer.Application.Extensions;

public static class MediaTagSetExtension
{
    extension(MediaTagSet tags)
    {
        // Whether the tags read from a file hold what the values say. Text is compared trimmed, with null and empty the same; lists in order with empty entries left out; a number that is not set is null or zero. The picture and the duration are not part of the values.
        public bool Matches(MediaTagValues values)
        {
            return IsSame(tags.Title, values.Title)
                   && IsSame(tags.Album, values.Album)
                   && IsSame(tags.Subtitle, values.Subtitle)
                   && IsSame(tags.Comment, values.Comment)
                   && IsSameList(tags.ArtistList, values.ArtistNameList)
                   && IsSameList(tags.AlbumArtistList, values.AlbumArtistNameList)
                   && IsSameList(tags.GenreList, values.GenreNameList)
                   && IsSameNumber(tags.TrackNumber, values.TrackNumber)
                   && IsSameNumber(tags.DiscNumber, values.DiscNumber)
                   && IsSameNumber(tags.Year, values.Year);
        }

        // The same, and when a picture is given the file's cover must be those bytes: the album's picture is what the file should carry. No picture given leaves the cover out of it.
        public bool Matches(MediaTagValues values, byte[]? picture)
        {
            return tags.Matches(values) && (picture is null || (tags.Picture is not null && tags.Picture.AsSpan().SequenceEqual(picture)));
        }
    }

    private static bool IsSame(string? first, string? second)
    {
        return string.Equals(first?.Trim() ?? string.Empty, second?.Trim() ?? string.Empty, StringComparison.Ordinal);
    }

    private static bool IsSameList(List<string> first, List<string> second)
    {
        return first.Select(x => x.Trim()).Where(x => x.Length > 0).SequenceEqual(second.Select(x => x.Trim()).Where(x => x.Length > 0), StringComparer.Ordinal);
    }

    private static bool IsSameNumber(int? first, int? second)
    {
        return (first ?? 0) == (second ?? 0);
    }
}
