using Noxy.NET.MediaPlayer.Application.Interfaces.Services;
using Noxy.NET.MediaPlayer.Domain.Entities;
using Noxy.NET.MediaPlayer.Domain.Extensions;
using TagLib;
using TagLib.Mpeg;
using File = TagLib.File;
using Tag = TagLib.Id3v2.Tag;

namespace Noxy.NET.MediaPlayer.Application.Services;

public class MediaTagWritingService : IMediaTagWritingService
{
    // Only ID3v2.4 holds several values in one frame: older versions would join them with a slash, which a name such as AC/DC contains.
    private const byte Id3V2VersionWithValueLists = 4;

    public void Write(string filePath, EntityMediaFile mediaFile)
    {
        EntityMediaFileMetadata metadata = mediaFile.Metadata ?? throw new ArgumentException($"The media file {mediaFile.ID} has no metadata.", nameof(mediaFile));

        using File file = File.Create(filePath);
        if (file is AudioFile && file.GetTag(TagTypes.Id3v2, true) is Tag id3) id3.Version = Id3V2VersionWithValueLists;

        TagLib.Tag tag = file.Tag;

        tag.Title = metadata.Title;
        tag.Performers = [.. mediaFile.GetEffectiveArtistNameList()];
        tag.Album = mediaFile.GetEffectiveAlbumTitle();
        tag.AlbumArtists = [.. mediaFile.GetEffectiveAlbumArtistNameList()];
        tag.Subtitle = metadata.Subtitle;
        tag.Comment = metadata.Comment;
        tag.Genres = [.. mediaFile.GetEffectiveGenreNameList()];
        tag.Track = ToTagNumber(metadata.TrackNumber);
        tag.Disc = ToTagNumber(metadata.DiscNumber);
        tag.Year = ToTagNumber(metadata.Year);

        file.Save();
    }

    // The tag formats use zero for "not set".
    private static uint ToTagNumber(int? value)
    {
        return (uint)Math.Max(0, value ?? 0);
    }
}
