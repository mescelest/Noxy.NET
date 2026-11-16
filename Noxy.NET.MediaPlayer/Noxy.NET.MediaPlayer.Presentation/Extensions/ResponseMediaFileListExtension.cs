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
            string[] wordList = (query ?? string.Empty).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            return wordList.Length == 0
                ? [.. mediaFileList]
                : mediaFileList.Where(mediaFile => wordList.All(word => Matches(mediaFile, word))).ToList();
        }

        // Stable: equal keys keep their order, in both directions. A media file with no key (no artist, album or year) always comes last. Always a new list.
        public List<ResponseMediaFile> Sort(LibrarySortEnum sort, bool descending)
        {
            return sort switch
            {
                LibrarySortEnum.Title => SortByText(mediaFileList, x => x.Title, descending),
                LibrarySortEnum.Artist => SortByText(mediaFileList, x => x.Artist, descending),
                LibrarySortEnum.Album => SortByText(mediaFileList, x => x.Album, descending),
                LibrarySortEnum.Year => SortByNumber(mediaFileList, x => x.Year, descending),
                LibrarySortEnum.TimeCreated => SortByNumber(mediaFileList, x => x.TimeCreated.Ticks, descending),
                _ => throw new InvalidEnumArgumentException(nameof(sort), (int)sort, typeof(LibrarySortEnum))
            };
        }
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

    private static List<ResponseMediaFile> SortByText(List<ResponseMediaFile> mediaFileList, Func<ResponseMediaFile, string?> key, bool descending)
    {
        List<ResponseMediaFile> keyed = mediaFileList.Where(x => key(x) is not null).ToList();
        List<ResponseMediaFile> unkeyed = mediaFileList.Where(x => key(x) is null).ToList();

        keyed = descending
            ? keyed.OrderByDescending(key, StringComparer.OrdinalIgnoreCase).ToList()
            : keyed.OrderBy(key, StringComparer.OrdinalIgnoreCase).ToList();

        return [.. keyed, .. unkeyed];
    }

    private static List<ResponseMediaFile> SortByNumber(List<ResponseMediaFile> mediaFileList, Func<ResponseMediaFile, long?> key, bool descending)
    {
        List<ResponseMediaFile> keyed = mediaFileList.Where(x => key(x) is not null).ToList();
        List<ResponseMediaFile> unkeyed = mediaFileList.Where(x => key(x) is null).ToList();

        keyed = descending ? keyed.OrderByDescending(key).ToList() : keyed.OrderBy(key).ToList();

        return [.. keyed, .. unkeyed];
    }
}
