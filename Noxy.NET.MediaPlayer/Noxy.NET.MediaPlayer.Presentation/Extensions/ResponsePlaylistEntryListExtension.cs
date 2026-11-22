using Noxy.NET.MediaPlayer.Domain.Enums;
using Noxy.NET.MediaPlayer.Domain.Responses.Playlist;

namespace Noxy.NET.MediaPlayer.Presentation.Extensions;

public static class ResponsePlaylistEntryListExtension
{
    extension(List<ResponsePlaylistEntry> entryList)
    {
        // What the playlist page shows: the entries filtered and sorted like the Library's media files, each entry kept as its own row. The stored order when there is no sort. Always a new list.
        public List<ResponsePlaylistEntry> ToDisplayList(string? search, LibrarySortEnum? sort, bool descending)
        {
            List<ResponsePlaylistEntry> filtered = ResponseMediaFileListExtension.FilterBy(entryList, search, x => x.MediaFile);
            return sort is { } chosen ? ResponseMediaFileListExtension.SortBy(filtered, chosen, descending, x => x.MediaFile) : filtered;
        }
    }

    // Dragging reorders the stored playlist, so it is only allowed while the rows on screen are the stored playlist.
    public static bool CanReorder(string? search, LibrarySortEnum? sort)
    {
        return sort is null && string.IsNullOrWhiteSpace(search);
    }
}
