using Noxy.NET.MediaPlayer.Domain.Models;
using Noxy.NET.MediaPlayer.Domain.Responses.Media;

namespace Noxy.NET.MediaPlayer.Domain.Extensions;

public static class ResponseMediaFileExtension
{
    extension(ResponseMediaFile mediaFile)
    {
        // The tags the media file's metadata stands for. Artists, album artists and genres are looked up by ID in the maps, in the order of the lists, so a name is never cut out of joined text. Null when an ID is not in its map: the caller does not have that item yet.
        public MediaTagValues? ToTagValues(IReadOnlyDictionary<Guid, string> artistNameMap, IReadOnlyDictionary<Guid, string> genreNameMap)
        {
            List<string>? artistNameList = Resolve(mediaFile.ArtistIDList, artistNameMap);
            List<string>? albumArtistNameList = Resolve(mediaFile.AlbumArtistIDList, artistNameMap);
            List<string>? genreNameList = Resolve(mediaFile.GenreIDList, genreNameMap);
            if (artistNameList is null || albumArtistNameList is null || genreNameList is null) return null;

            return new(mediaFile.Title, artistNameList, mediaFile.Album, albumArtistNameList, mediaFile.Subtitle, mediaFile.Comment, genreNameList, mediaFile.TrackNumber, mediaFile.DiscNumber, mediaFile.Year);
        }
    }

    private static List<string>? Resolve(List<Guid> idList, IReadOnlyDictionary<Guid, string> nameMap)
    {
        List<string> nameList = [];
        foreach (Guid id in idList)
        {
            if (!nameMap.TryGetValue(id, out string? name)) return null;

            nameList.Add(name);
        }

        return nameList;
    }
}
