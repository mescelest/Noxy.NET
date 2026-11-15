using Microsoft.Extensions.Logging;
using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Application.Models.Metadata;
using TagLib;
using File = TagLib.File;

namespace Noxy.NET.MediaPlayer.Application.Services;

public class MediaTagReadingService(ILogger<MediaTagReadingService> logger) : IMediaTagReadingService
{
    private const char Id3V23Separator = '/';
    private const byte FirstId3V2VersionWithValueLists = 4;

    public MediaTagSet Read(string path)
    {
        try
        {
            using File file = File.Create(path);
            Tag tag = file.Tag;
            TimeSpan duration = file.Properties?.Duration ?? TimeSpan.Zero;

            // ID3v2.2 and 2.3 have no list of values: TagLib splits a text frame on "/", which would break "AC/DC", so the pieces are put back together.
            bool slashIsPartOfName = file.GetTag(TagTypes.Id3v2, false) is TagLib.Id3v2.Tag { Version: < FirstId3V2VersionWithValueLists };

            return new(
                tag.Title,
                ToValueList(tag.Performers, slashIsPartOfName),
                tag.Album,
                ToValueList(tag.AlbumArtists, slashIsPartOfName),
                ToValueList(tag.Genres, slashIsPartOfName),
                ToNumber(tag.Track),
                ToNumber(tag.Disc),
                ToNumber(tag.Year),
                tag.Comment,
                tag.Subtitle,
                duration > TimeSpan.Zero ? duration : null);
        }
        catch (Exception exception)
        {
            // The exception's type only: its message can quote what was in the file.
            logger.LogWarning("The tags of a media file could not be read ({ExceptionType}).", exception.GetType().Name);

            return MediaTagSet.Empty;
        }
    }

    private static List<string> ToValueList(string[] valueList, bool slashIsPartOfName)
    {
        return slashIsPartOfName && valueList.Length > 1 ? [string.Join(Id3V23Separator, valueList)] : [.. valueList];
    }

    // The tag formats use zero for "not set".
    private static int? ToNumber(uint value)
    {
        return value == 0 ? null : (int)Math.Min(value, int.MaxValue);
    }
}
