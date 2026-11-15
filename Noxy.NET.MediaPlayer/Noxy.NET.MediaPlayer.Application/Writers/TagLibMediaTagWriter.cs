using Noxy.NET.MediaPlayer.Application.Interfaces.Writers;
using Noxy.NET.MediaPlayer.Domain.Entities;
using TagLib;
using File = TagLib.File;

namespace Noxy.NET.MediaPlayer.Application.Writers;

public class TagLibMediaTagWriter : IMediaTagWriter
{
    public void Write(string filePath, EntityMediaFileMetadata metadata)
    {
        using File file = File.Create(filePath);
        Tag tag = file.Tag;

        tag.Title = metadata.Title;
        tag.Performers = metadata.Artist is null ? [] : [metadata.Artist];
        tag.Album = metadata.Album;
        tag.AlbumArtists = metadata.AlbumArtist is null ? [] : [metadata.AlbumArtist];
        tag.Subtitle = metadata.Subtitle;
        tag.Comment = metadata.Comment;
        tag.Genres = metadata.Genre is null ? [] : [metadata.Genre];
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
