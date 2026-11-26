using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Domain.Models;
using TagLib;
using TagLib.Id3v2;
using TagLib.Mpeg;
using File = TagLib.File;
using Tag = TagLib.Id3v2.Tag;

namespace Noxy.NET.MediaPlayer.Application.Services;

public class MediaTagWritingService : IMediaTagWritingService
{
    // Only ID3v2.4 holds several values in one frame: older versions would join them with a slash, which a name such as AC/DC contains.
    private const byte Id3V2VersionWithValueLists = 4;

    private static readonly ByteVector FrameGenre = "TCON";

    private const string CoverMimeType = "image/jpeg";

    public void Write(string filePath, MediaTagValues values, byte[]? picture = null)
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

        // TagLib turns a name from the ID3v1 list ("Rock") into its number ("17") in an ID3v2 frame, which other apps may show as the number: the frame is written as the names.
        if (values.GenreNameList.Count > 0 && file.GetTag(TagTypes.Id3v2, false) is Tag genreTag) TextInformationFrame.Get(genreTag, FrameGenre, true).Text = [.. values.GenreNameList];
        tag.Track = ToTagNumber(values.TrackNumber);
        tag.TrackCount = values.TrackNumber is null ? 0 : tag.TrackCount;
        tag.Disc = ToTagNumber(values.DiscNumber);
        tag.DiscCount = values.DiscNumber is null ? 0 : tag.DiscCount;
        tag.Year = ToTagNumber(values.Year);

        if (picture is not null) tag.Pictures = [new Picture(new ByteVector(picture)) { Type = PictureType.FrontCover, MimeType = CoverMimeType }];

        file.Save();
    }

    // The tag formats use zero for "not set". A track or disc that is not set also drops its total: a total left alone keeps the frame, as "00/12".
    private static uint ToTagNumber(int? value)
    {
        return (uint)Math.Max(0, value ?? 0);
    }
}
