using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Domain.Models;
using TagLib;
using TagLib.Mpeg;
using File = TagLib.File;
using Tag = TagLib.Id3v2.Tag;

namespace Noxy.NET.MediaPlayer.Application.Services;

public class MediaTagWritingService : IMediaTagWritingService
{
    // Only ID3v2.4 holds several values in one frame: older versions would join them with a slash, which a name such as AC/DC contains.
    private const byte Id3V2VersionWithValueLists = 4;

    public void Write(string filePath, MediaTagValues values)
    {
        using File file = File.Create(filePath);
        if (file is AudioFile && file.GetTag(TagTypes.Id3v2, true) is Tag id3) id3.Version = Id3V2VersionWithValueLists;

        TagLib.Tag tag = file.Tag;

        tag.Title = values.Title;
        tag.Performers = [.. values.ArtistNameList];
        tag.Album = values.Album;
        tag.AlbumArtists = [.. values.AlbumArtistNameList];
        tag.Subtitle = values.Subtitle;
        tag.Comment = values.Comment;
        tag.Genres = [.. values.GenreNameList];
        tag.Track = ToTagNumber(values.TrackNumber);
        tag.Disc = ToTagNumber(values.DiscNumber);
        tag.Year = ToTagNumber(values.Year);

        file.Save();
    }

    // The tag formats use zero for "not set".
    private static uint ToTagNumber(int? value)
    {
        return (uint)Math.Max(0, value ?? 0);
    }
}
