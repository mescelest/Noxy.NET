using System.ComponentModel;
using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;

namespace Noxy.NET.MediaPlayer.Presentation.Extensions;

public static class ResponseMediaFileListExtension
{
    extension(List<ResponseMediaFile> mediaFileList)
    {
        // Every word of the query must be found (case-insensitive substring, accents as typed) in the title, artist, album, album artist or genre. A blank query returns everything. Always a new list.
        public List<ResponseMediaFile> Filter(string? query)
        {
            return FilterBy(mediaFileList, query, x => x);
        }

        // Stable: equal keys keep their order, in both directions. A media file with no key (no artist, album or year) always comes last. Always a new list.
        public List<ResponseMediaFile> Sort(LibrarySortEnum sort, bool descending)
        {
            return SortBy(mediaFileList, sort, descending, x => x);
        }

        // A null kind is not filtered; a file with no item of a filtered kind is dropped. Always a new list.
        public List<ResponseMediaFile> FilterByMetadata(Guid? artistID, Guid? albumID, Guid? genreID)
        {
            return mediaFileList
                .Where(x => artistID is null || x.ArtistIDList.Contains(artistID.Value))
                .Where(x => albumID is null || x.AlbumID == albumID)
                .Where(x => genreID is null || x.GenreIDList.Contains(genreID.Value))
                .ToList();
        }
    }

    // The same filter for any list whose items carry a media file, such as the entries of a playlist.
    internal static List<T> FilterBy<T>(List<T> list, string? query, Func<T, ResponseMediaFile> mediaFileOf)
    {
        string[] wordList = (query ?? string.Empty).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        return wordList.Length == 0
            ? [.. list]
            : list.Where(item => wordList.All(word => Matches(mediaFileOf(item), word))).ToList();
    }

    internal static List<T> SortBy<T>(List<T> list, LibrarySortEnum sort, bool descending, Func<T, ResponseMediaFile> mediaFileOf)
    {
        return sort switch
        {
            LibrarySortEnum.Title => SortByText(list, x => mediaFileOf(x).Title, descending),
            LibrarySortEnum.Artist => SortByText(list, x => mediaFileOf(x).Artist, descending),
            LibrarySortEnum.Album => SortByText(list, x => mediaFileOf(x).Album, descending),
            LibrarySortEnum.Genre => SortByText(list, x => mediaFileOf(x).Genre, descending),
            LibrarySortEnum.Year => SortByNumber(list, x => mediaFileOf(x).Year, descending),
            LibrarySortEnum.TimeCreated => SortByNumber(list, x => mediaFileOf(x).TimeCreated.Ticks, descending),
            _ => throw new InvalidEnumArgumentException(nameof(sort), (int)sort, typeof(LibrarySortEnum))
        };
    }

    private static bool Matches(ResponseMediaFile mediaFile, string word)
    {
        return Contains(mediaFile.Title, word)
               || Contains(mediaFile.Artist, word)
               || Contains(mediaFile.Album, word)
               || Contains(mediaFile.AlbumArtist, word)
               || Contains(mediaFile.Genre, word);
    }

    private static bool Contains(string? field, string word)
    {
        return field is not null && field.Contains(word, StringComparison.OrdinalIgnoreCase);
    }

    private static List<T> SortByText<T>(List<T> list, Func<T, string?> key, bool descending)
    {
        List<T> keyed = list.Where(x => key(x) is not null).ToList();
        List<T> unkeyed = list.Where(x => key(x) is null).ToList();

        keyed = descending
            ? keyed.OrderByDescending(key, StringComparer.OrdinalIgnoreCase).ToList()
            : keyed.OrderBy(key, StringComparer.OrdinalIgnoreCase).ToList();

        return [.. keyed, .. unkeyed];
    }

    private static List<T> SortByNumber<T>(List<T> list, Func<T, long?> key, bool descending)
    {
        List<T> keyed = list.Where(x => key(x) is not null).ToList();
        List<T> unkeyed = list.Where(x => key(x) is null).ToList();

        keyed = descending ? keyed.OrderByDescending(key).ToList() : keyed.OrderBy(key).ToList();

        return [.. keyed, .. unkeyed];
    }
}
